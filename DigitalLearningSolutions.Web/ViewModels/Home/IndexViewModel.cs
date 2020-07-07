﻿namespace DigitalLearningSolutions.Web.ViewModels.Home
{
    using System.Collections.Generic;
    using DigitalLearningSolutions.Data.Models;

    public class IndexViewModel
    {
        private readonly HeadlineFigures headlineFigures;

        public IndexViewModel(HeadlineFigures headlineFigures)
        {
            this.headlineFigures = headlineFigures;
        }

        public IEnumerable<HeadlineFigureViewModel> HeadlineFigures
        {
            get
            {
                if (headlineFigures == null)
                {
                    yield break;
                }

                yield return new HeadlineFigureViewModel
                {
                    Label = "Centres",
                    Value = headlineFigures.ActiveCentres
                };
                yield return new HeadlineFigureViewModel
                {
                    Label = "Learners",
                    Value = headlineFigures.Delegates
                };
                yield return new HeadlineFigureViewModel
                {
                    Label = "Learning Hours",
                    Value = headlineFigures.LearningTime
                };
                yield return new HeadlineFigureViewModel
                {
                    Label = "Courses Completed",
                    Value = headlineFigures.Completions
                };
            }
        }

        public class HeadlineFigureViewModel
        {
            public string Label { get; set; }
            public string CssClassname => Label.ToLower().Replace(' ', '-');
            public int Value { get; set; }
        }
    }
}