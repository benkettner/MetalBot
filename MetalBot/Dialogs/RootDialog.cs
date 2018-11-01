using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MetalBot.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace MetalBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        State myState = State.INITIATED;
        Dictionary<Drink, int> LabelDetection;
        string userName;
        const string subscriptionKey = "<Subscription Key>";

        public async Task StartAsync(IDialogContext context)
        {
            var activity = context.Activity;
            if (activity.From.Name != null)
            {
                userName = activity.From.Name.Split(' ')[0];
            }
            await context.PostAsync($"{MessageUtil.GetGreeting()}, {userName}. ");
            context.Wait(MessageReceivedAsync);

            await Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var res = await result;
            var activity = res as Activity;
            if (myState == State.FINDING_BANDS)
            {
                Style s = MessageUtil.StringToStyle(res.ToString());
                if (s != Style.POP)
                {
                    await context.PostAsync($"Meine Algorithmen haben die perfekte Unterhaltung für Dich gefunden: {MessageUtil.GetBandForStyle(s)} oder die Key Performance Indicators würden Ideal zu Deinem Geschmack passen.");
                } else
                {
                    await context.PostAsync($"Meine Algorithmen finden nicht gut, was Du gut findest, vermutlich stehst Du auf {MessageUtil.GetBandForStyle(s)}.");
                }
                myState = State.INITIATED;
                await context.PostAsync($"Nochmal?");
            }
            else if (myState == State.GIVING_RECOMMENDATION)
            {
                List<Style> myStyles = new List<Style>();
                foreach (Drink d in LabelDetection.Keys)
                {
                    myStyles.AddRange(MessageUtil.GetStylesForDrink(d, 3));
                }

                List<string> styleSelection = new List<string>();
                foreach (Style s in myStyles)
                {
                    styleSelection.Add(MessageUtil.StyleToString(s));
                }
                myState = State.FINDING_BANDS;
                ShowOptions(context, "Welcher davon darf's denn sein?", styleSelection);

            }
            else if (myState == State.AWAITING_IMAGE)
            {
                if (activity.Attachments != null && activity.Attachments.Any())
                {
                    bool continueEvaluation = true;
                    var attachment = activity.Attachments.First();
                    if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
                    {
                        string ImageText = await MessageUtil.DetectTextInImageAsync(attachment.ContentUrl);
                        LabelDetection = MessageUtil.FindDrinkInLabelText(ImageText);
                        string ResponseText;
                        if (LabelDetection.Count() == 0)
                        {
                            ResponseText = $"Ich konnte da kein Getränk entdecken, bist Du sicher, dass Du nicht versucht hast, mir ein Nacktbild von Dir unterzujubeln? Probiere es doch nochmal.";
                            continueEvaluation = false;
                        }
                        else if (LabelDetection.Count() == 1)
                        {
                            ResponseText = $"Ich konnte hier {MessageUtil.DrinkToString(LabelDetection.ElementAt(0).Key) } entdecken.";
                        }
                        else
                        {
                            string DrinksAnalysis = "";
                            int TotalLabels = LabelDetection.Sum(x => x.Value);
                            foreach (KeyValuePair<Drink, int> kvp in LabelDetection.OrderByDescending(x => x.Value))
                            {
                                DrinksAnalysis += (MessageUtil.DrinkToString(kvp.Key) + $" mit einer Wahrscheinlichkeit von {((double)kvp.Value/(double)TotalLabels):F2}%, ");
                            }
                            ResponseText = $"Ich konnte mehrere Getränke sehen: {DrinksAnalysis.Substring(DrinksAnalysis.Length-3)}. Ich entscheide mich für {MessageUtil.DrinkToString(LabelDetection.OrderByDescending(x => x.Value).First().Key)}.";
                        }
                        await context.PostAsync(ResponseText);
                        if (continueEvaluation)
                        {
                            myState = State.GIVING_RECOMMENDATION;
                            ShowOptions(context, "Jetzt möchtest Du doch sicher wissen, was Du da am besten hören solltest, oder?", MessageUtil.GetPositiveResponses(3));
                        }
                    }
                    else
                    {
                        await context.PostAsync("Ich brauche ein Bild.");
                    }
                }
                else
                {
                    await context.PostAsync("Ich brauche ein Bild.");
                }
            }
            else if (myState == State.IMAGE_TRIGGERED)
            {
                myState = State.AWAITING_IMAGE;
                await context.PostAsync("Damit ich Dir helfen kann, schick mir ein Bild von Deinem Lieblingsgetränk.");
            }
            else
            {
                await context.PostAsync("Ich kann Dir die eine, die Ultimative und die einzig entscheidende Frage beantworten.");
                myState = State.IMAGE_TRIGGERED;
                ShowOptions(context, "Möchtest Du wissen, was für eine Band du hören solltest?", MessageUtil.GetPositiveResponses(3));
                //await context.PostAsync(MyResponse);
            }
        }


        private void ShowOptions(IDialogContext context, string messagetext, List<string> Options)
        {
            PromptDialog.Choice(context,
                this.MessageReceivedAsync,
                Options,
                messagetext,
                "Was soll ich denn DAMIT anfangen?", 3);
        }
    }
}