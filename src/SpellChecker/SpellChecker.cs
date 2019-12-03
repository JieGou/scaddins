﻿namespace SCaddins.SpellChecker
{
    using System.Collections;
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using NHunspell;
    using SCaddins;

    public class SpellChecker : IEnumerator
    {
        private List<CorrectionCandidate> allTextParameters;
        private Dictionary<string, string> autoReplacementList = new Dictionary<string, string>();
        private int currentIndex;
        private Document document;
        private Hunspell hunspell;

        public SpellChecker(Document document)
        {
            if (hunspell != null)
            {
                hunspell.Dispose();
            }

            this.document = document;

            string dll = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            try
            {
                if (Hunspell.NativeDllPath != dll)
                {
                    Hunspell.NativeDllPath = dll;
                }
            } catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            #if DEBUG
            //// SCaddinsApp.WindowManager.ShowMessageBox(System.IO.Path.Combine(dll, "Assets"));
            hunspell = new Hunspell(
                            System.IO.Path.Combine(dll, @"Assets/en_AU.aff"),
                            System.IO.Path.Combine(dll, @"Assets/en_AU.dic"));
            #else
            hunspell = new Hunspell(
                            System.IO.Path.Combine(Constants.InstallDirectory, "etc", "en_AU.aff"),
                            System.IO.Path.Combine(Constants.InstallDirectory, "etc", "en_AU.dic"));
            #endif

            // add some arch specific words
            hunspell.Add("approver");
            hunspell.Add(@"&");
            hunspell.Add(@"-");
            hunspell.Add(@"Autodesk");

            allTextParameters = GetAllTextParameters(document);
            currentIndex = -1;
        }

        ~SpellChecker()
        {
            hunspell.Dispose();
        }

        /// <summary>
        /// Return the current CorrectionCandidate object
        /// </summary>
        public object Current => allTextParameters[SafeCurrentIndex];

        /// <summary>
        /// Returns the curent CorrectionCandiate
        /// </summary>
        public CorrectionCandidate CurrentCandidate => (CorrectionCandidate)Current;

        public string CurrentElementType => CurrentCandidate.TypeString;

        /// <summary>
        /// Returns the current unknown word.
        /// </summary>
        public string CurrentUnknownWord => CurrentCandidate.Current as string;

        private int SafeCurrentIndex => currentIndex < allTextParameters.Count ? currentIndex : allTextParameters.Count - 1;

        public void AddToAutoReplacementList(string word, string replacement)
        {
            if (autoReplacementList.ContainsKey(word)) {
                return;
            }
            autoReplacementList.Add(word, replacement);
        }

        public void CommitSpellingChangesToModel()
        {
            int fails = 0;
            int successes = 0;

            using (var t = new Transaction(document)) {
                if (t.Start("Spelling") == TransactionStatus.Started) {
                    foreach (CorrectionCandidate candidate in allTextParameters) {
                        if (candidate.IsModified) {
                            if (candidate.Rename()) {
                                successes++;
                            } else {
                                fails++;
                            }
                        }
                    }
                    t.Commit();
                    SCaddinsApp.WindowManager.ShowMessageBox(
                        @"Spelling", successes + @" parameters succesfully renamed, " + fails + @" errors.");
                } else {
                    SCaddinsApp.WindowManager.ShowMessageBox("Error", "Failed to start Spelling Transaction...");
                }
            }
        }

        /// <summary>
        /// get spelling suggestions for the current CorrectionCandidate
        /// </summary>
        /// <returns></returns>
        public List<string> GetCurrentSuggestions()
        {
            if (currentIndex < 0) {
                return new List<string>();
            }
            if (hunspell != null && allTextParameters.Count > 0 && currentIndex < allTextParameters.Count) {
                return hunspell.Suggest(allTextParameters[currentIndex].Current as string);
            }
            return new List<string>();
        }

        /// <summary>
        /// Ingnore all future instances of the CurrentUnknownWord
        /// </summary>
        public void IgnoreAll()
        {
            hunspell.Add(CurrentUnknownWord);
        }

        public bool MoveNext()
        {
            if (allTextParameters == null || allTextParameters.Count <= 0) {
                return false;
            }
            while (currentIndex < allTextParameters.Count) {
                if (currentIndex == -1)
                {
                    currentIndex = 0;
                }
                if (allTextParameters[currentIndex].MoveNext()) {
                    return true;
                }
                currentIndex++;
            }
            return false;
        }

        public void Reset()
        {
            currentIndex = -1;
        }

        /// <summary>
        /// Get all user modifiable parameters in the revit doc.
        /// Only get parameters of string storage types, as there's not much point spell cheking numbers.
        /// 
        /// </summary>
        /// <param name="doc">Revit doc to spell check</param>
        /// <returns>parmaeters</returns>
        private List<CorrectionCandidate> GetAllTextParameters(Document doc)
        {
            var candidates = new List<CorrectionCandidate>();
            var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                var parameterSet = element.Parameters;
                if (parameterSet == null || parameterSet.IsEmpty) {
                    continue;
                }
                foreach (var parameter in parameterSet)
                {
                    if (parameter is Autodesk.Revit.DB.Parameter)
                    {
                        Autodesk.Revit.DB.Parameter p = (Autodesk.Revit.DB.Parameter)parameter;
                        if (p == null || !p.HasValue) {
                            continue;
                        }
                        if (p.IsReadOnly) {
                            continue;
                        }
                        try
                        {
                            if (p.StorageType == StorageType.String)
                            {
                                var rc = new CorrectionCandidate(p, hunspell, ref autoReplacementList);

                                if (!string.IsNullOrEmpty(rc.OriginalText))
                                {
                                    candidates.Add(rc);
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.Message);
                        }
                    }
                }
            }
            return candidates;
        }
    }
}
