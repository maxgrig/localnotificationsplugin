﻿using System;
using Plugin.LocalNotifications.Abstractions;
using System.Linq;
using Foundation;
using UIKit;
using UserNotifications;

namespace Plugin.LocalNotifications
{
    /// <summary>
    /// Local Notifications implementation for iOS
    /// </summary>
    public class LocalNotificationsImplementation : ILocalNotifications
    {
        public const string NotificationDataKey = "notification_data";

        private const string NotificationKey = "LocalNotificationKey";

        /// <summary>
        /// Show a local notification
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        public void Show(string title, string body, int id = 0, string data = null)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(.1, false);
                ShowUserNotification(title, body, id, trigger, data);
            }
            else
            {
                Show(title, body, id, DateTime.Now);
            }
        }

        /// <summary>
        /// Show a local notification at a specified time
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        /// <param name="notifyTime">Time to show notification</param>
        public void Show(string title, string body, int id, DateTime notifyTime, string data = null)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponentsFromDateTime(notifyTime), false);
                ShowUserNotification(title, body, id, trigger, data);
            }
            else
            {
                var notification = new UILocalNotification
                {
                    FireDate = (NSDate)notifyTime,
                    AlertTitle = title,
                    AlertBody = body, 
                    UserInfo = NSDictionary.FromObjectsAndKeys(
                        new[] { NSObject.FromObject(id),              NSObject.FromObject(data) },
                        new[] { NSObject.FromObject(NotificationKey), NSObject.FromObject(NotificationDataKey) })
                };

                UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            }
        }

        /// <summary>
        /// Cancel a local notification
        /// </summary>
        /// <param name="id">Id of the notification to cancel</param>
        public void Cancel(int id)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new string[] { id.ToString() });
                UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new string[] { id.ToString() });
            }
            else
            {
                var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
                var notification = notifications.Where(n => n.UserInfo.ContainsKey(NSObject.FromObject(NotificationKey)))
                    .FirstOrDefault(n => n.UserInfo[NotificationKey].Equals(NSObject.FromObject(id)));

                if (notification != null)
                {
                    UIApplication.SharedApplication.CancelLocalNotification(notification);
                }
            }
        }

        // Show local notifications using the UNUserNotificationCenter using a notification trigger (iOS 10+ only)
        void ShowUserNotification(string title, string body, int id, UNNotificationTrigger trigger, string data = null)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                return;
            }

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = body
            };

            if (data != null)
            {
                content.UserInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(data), NSObject.FromObject(NotificationDataKey));
            }

            var request = UNNotificationRequest.FromIdentifier(id.ToString(), content, trigger);

            var center = UNUserNotificationCenter.Current;
            center.AddNotificationRequest(request, (error) => { });
        }

        NSDateComponents GetNSDateComponentsFromDateTime(DateTime dateTime)
        {
            return new NSDateComponents
            {
                Month = dateTime.Month,
                Day = dateTime.Day,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second
            };
        }
    }
}
