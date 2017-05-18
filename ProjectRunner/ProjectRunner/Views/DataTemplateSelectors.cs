using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.Views.Selectors
{
    public class SportTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RunningTemplate { get; set; }
        public DataTemplate FootballTemplate { get; set; }
        public DataTemplate BicycleTemplate { get; set; }
        public DataTemplate TennisTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is RunningActivity)
                return RunningTemplate;
            if (item is FootballActivity)
                return FootballTemplate;
            if (item is BicycleActivity)
                return BicycleTemplate;
            if (item is TennisActivity)
                return TennisTemplate;
            return null;
        }
    }
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MyMessage { get; set; }
        public DataTemplate UserMessage { get; set; }
        public DataTemplate ServiceMessage { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var message = item as ChatMessage;
            if (message.MessageType == ChatMessage.ChatMessageType.USER)
                return message.IsMine ? MyMessage : UserMessage;
            return ServiceMessage;
        }
    }
}
