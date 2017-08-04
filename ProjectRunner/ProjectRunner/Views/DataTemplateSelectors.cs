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
            if (item is ChatMessage message)
            {
                if (message.MessageType == ChatMessage.ChatMessageType.USER)
                    return message.IsMine ? MyMessage : UserMessage;
                return ServiceMessage;
            }
            return null;
        }
    }
    public class ActivityUserProfileImageSelector : DataTemplateSelector
    {
        public DataTemplate AnonMale { get; set; }
        public DataTemplate AnonFemale { get; set; }
        public DataTemplate ImageProfile { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is UserProfile profile)
            {
                if (!string.IsNullOrEmpty(profile.Image))
                    return ImageProfile;

                if (profile.Sex == 0)
                    return AnonMale;
                else
                    return AnonFemale;
            }
            return null;
        }
    }
}
