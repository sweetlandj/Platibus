using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Platibus.Journaling;

namespace Platibus.SampleWebApp.Models
{
    public class MessageJournalIndexModel
    {
        private IList<SelectListItem> _allTopics;
        private IList<SelectListItem> _allCategories;
        private IList<string> _topics;
        private IList<string> _categories;

        public string Start { get; set; }
        public int Count { get; set; }
        public bool ReadAttempted { get; set; }

        public IList<SelectListItem> AllTopics
        {
            get { return _allTopics ?? (_allTopics = new List<SelectListItem>()); }
            set { _allTopics = value; }
        }

        public IList<SelectListItem> AllCategories
        {
            get { return _allCategories ?? (_allCategories = new List<SelectListItem>()); }
            set { _allCategories = value; }
        }

        [Display(Name = "Topics")]
        public IList<string> FilterTopics
        {
            get { return _topics ?? (_topics = new List<string>()); }
            set { _topics = value; }
        }

        [Display(Name = "Categories")]
        public IList<string> FilterCategories
        {
            get { return _categories ?? (_categories = new List<string>()); }
            set { _categories = value; }
        }

        public MessageJournalReadResult Result { get; set; }
       
    }
}