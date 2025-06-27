using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Data.Models
{
    /// <summary>
    /// Represents different types of activities or personnel categories.
    /// </summary>
    public enum ActivityType
    {
        Management = 0,
        Teachers = 1,
        Educators = 2,
        Caregivers = 3,
        Tutors = 4,
        Support = 5,
        Service = 6,
        Other = 7
    }
    /// <summary>
    /// Provides extension methods for the <see cref="ActivityType"/> enum.
    /// </summary>
    public static class ActivityTypeExtensions
    {
        /// <summary>
        /// Converts an <see cref="ActivityType"/> value to its corresponding Russian title string.
        /// </summary>
        /// <param name="activityType">The activity type to convert.</param>
        /// <returns>A localized title string representing the activity type.</returns>
        public static string ToTitle(this ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.Management:
                    return "Администрация";
                case ActivityType.Teachers:
                    return "Учителя";
                case ActivityType.Educators:
                    return "Педагоги";
                case ActivityType.Caregivers:
                    return "Воспитатели";
                case ActivityType.Tutors:
                    return "Преподаватели";
                case ActivityType.Support:
                    return "Вспомогательный персонал";
                case ActivityType.Service:
                    return "Обслуживающий персонал";
                case ActivityType.Other:
                    return "Прочий персонал";
                default:
                    return activityType.ToString();
            }
        }
    }
    /// <summary>
    /// Represents an activity item with an identifier, name, and localized title.
    /// </summary>
    public class ActivityItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the activity.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the activity (enum name).
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the localized title of the activity.
        /// </summary>
        public string Title { get; set; }
    }
    /// <summary>
    /// Provides helper methods related to activities and their mappings to positions.
    /// </summary>
    public static class ActivityHelper
    {
        /// <summary>
        /// Maps activity IDs to lists of position IDs associated with each activity.
        /// </summary>
        private static readonly Dictionary<int, List<int>> activityPosMap = new Dictionary<int, List<int>>
        {
            { 0, new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 } },
            { 1, new List<int> { 1 } },
            { 2, new List<int> { 3, 4, 6, 19 } },
            { 3, new List<int> { 5 } },
            { 4, new List<int> { 18 } },
            { 5, new List<int> { 18 } },
            { 6, new List<int> { } },
            { 7, new List<int> { 20 } },
        };
        /// <summary>
        /// Gets the <see cref="ActivityType"/> associated with a given position ID.
        /// </summary>
        /// <param name="positionId">The position identifier.</param>
        /// <returns>The corresponding <see cref="ActivityType"/> if found; otherwise, <see cref="ActivityType.Other"/>.</returns>
        public static ActivityType GetActivityFromPosition(int positionId)
        {
            foreach (var kvp in activityPosMap)
            {
                if (kvp.Value.Contains(positionId))
                {
                    return (ActivityType)kvp.Key;
                }
            }
            // Return Other if not found in dictionary
            return ActivityType.Other;
        }
        /// <summary>
        /// Retrieves a list of all activities defined in the <see cref="ActivityType"/> enum,
        /// including their IDs, names, and localized titles.
        /// </summary>
        /// <returns>A list of <see cref="ActivityItem"/> representing all activity types.</returns>
        public static List<ActivityItem> GetAllActivities()
        {
            return Enum.GetValues(typeof(ActivityType))
                       .Cast<ActivityType>()
                       .Select(at => new ActivityItem
                       {
                           Id = (int)at,
                           Name = at.ToString(),
                           Title = at.ToTitle()
                       })
                       .ToList();
        }
    }
    /// <summary>
    /// Represents a position with properties to access its associated activity information.
    /// </summary>
    public partial class Position
    {
        /// <summary>
        /// Gets the activity ID associated with this position.
        /// </summary>
        public int ActivityId => (int)ActivityHelper.GetActivityFromPosition(Id);
        /// <summary>
        /// Gets the activity name (enum name) associated with this position.
        /// </summary>
        public string ActivityName => ActivityHelper.GetActivityFromPosition(Id).ToString();
        /// <summary>
        /// Gets the localized activity title associated with this position.
        /// </summary>
        public string ActivityTitle => ActivityHelper.GetActivityFromPosition(Id).ToTitle();
    }
}
