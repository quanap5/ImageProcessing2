using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Tesseract;

namespace WpfApp2.Models
{
    public class TesseractOCR : ITesseractOCR
    {
        public string executedTime;
        private ViewModels.OCRViewModel taskViewModel;

        #region Mode of language
        private TesseractEngine ocrEnglish;
        private TesseractEngine ocrKorean;
        private TesseractEngine ocrJapanese;
        private TesseractEngine ocrAuto;
        private TesseractEngine curOcr;
        #endregion

        private Dictionary<SolidColorBrush, List<Rectangle>> ocrDetectedRegion;

        public TesseractOCR(ViewModels.OCRViewModel vm)
        {
            taskViewModel = vm;
            ocrEnglish = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            ocrKorean = new TesseractEngine("./tessdata", "kor", EngineMode.Default);
            ocrJapanese = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
            ocrAuto = new TesseractEngine("./tessdata", "eng+kor+jpn", EngineMode.Default);

        }

        public string LangORC { get; set; }
        

        public String getTime()
        {
            return this.executedTime;
        }
        public List<string> ListImageOCR(List<ImageClass> list)
        {
            throw new NotImplementedException();
        }

        public string OneImageOCR(ImageClass one)
        {
            if (one == null)
            {
                return string.Empty;

            }
            else
            {
                return runningOCR(one.FilePath);
            }
        }

        public void SelectLang(string selectedLang)
        {
            switch (selectedLang)
            {
                case "English":
                    LangORC = "eng";
                    curOcr = ocrEnglish;
                    break;
                case "Korean":
                    LangORC = "kor";
                    curOcr = ocrKorean;
                    break;
                case "Japanese":
                    LangORC = "jpn";
                    curOcr = ocrJapanese;
                    break;
                case "Auto":
                    LangORC = "eng+kor+jpn";
                    curOcr = ocrAuto;
                    break;
                default:
                    LangORC = "eng";
                    curOcr = ocrEnglish;
                    break;
            }

        }
        /// <summary>
        /// Click run button to start the OCR function
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string runningOCR(string filePath)
        {
            
            try
            {
                if (!File.Exists(filePath))
                {
                    //return "please open at least one image";
                    MessageBox.Show("Please open at least one image");
                    return null;

                }

                // Check the language if it be selected or not
                if (LangORC==null)
                {
                    MessageBox.Show("Please select language for recognizing");
                    return "Waiting for Select language";
                }
                else
                {
                    taskViewModel.OutPutText = "OCR is running....";
                    //Stopwatch stopW = Stopwatch.StartNew();
                    Console.WriteLine("Current language is: "+ LangORC);
                    //curOcr = new TesseractEngine("./tessdata", LangORC, EngineMode.Default);
                    Task.Run(() =>
                    {
                        for (int i = 0; i <= 100; i = i + 2)
                        {
                            Thread.Sleep(1);
                            //Console.WriteLine(i);
                            taskViewModel.LoadingBar = i;
                            //LoaddingBar = i;
                        }
                    });


                    //using (var ocr = new TesseractEngine("./tessdata", LangORC , EngineMode.Default))
                    //using (var ocr = curOcr)
                    {
                        Stopwatch stopW = Stopwatch.StartNew();
                        using (var img = Pix.LoadFromFile(filePath))
                        {
                            using (var page = curOcr.Process(img))
                            {
                                var resultText = page.GetText();
                                if (!String.IsNullOrEmpty(resultText))
                                {
                                    stopW.Stop();
                                    var time_dur = stopW.Elapsed.TotalMilliseconds.ToString();
                                    this.executedTime = time_dur;

                                    List<Rectangle> _charBoxs = page.GetSegmentedRegions(PageIteratorLevel.Symbol);
                                    List<Rectangle> _wordBoxs = page.GetSegmentedRegions(PageIteratorLevel.Word);
                                    List<Rectangle> _lineBoxs = page.GetSegmentedRegions(PageIteratorLevel.TextLine);
                                    List<Rectangle> _paraBoxs = page.GetSegmentedRegions(PageIteratorLevel.Para);
                                    Console.WriteLine("Number of Character is: "+ _charBoxs.Count);
                                    Console.WriteLine("Number of Word is: " + _wordBoxs.Count);
                                    Console.WriteLine("Number of Line is: " + _lineBoxs.Count);
                                    Console.WriteLine("Number of Paragraph is: " + _paraBoxs.Count);

                                    Console.WriteLine("Coordinate x of first word:" + _charBoxs[0].X + "y: " + _charBoxs[0].Y + "width: " + _charBoxs[0].Width + "Heigh: " + _charBoxs[0].Height);

                                    Console.WriteLine("Coordinate x of first word:"+ _wordBoxs[0].X +"y: "+_wordBoxs[0].Y + "width: "+_wordBoxs[0].Width + "Heigh: "+_wordBoxs[0].Height);

                                    Console.WriteLine("Coordinate x of first word:" + _lineBoxs[0].X + "y: " + _lineBoxs[0].Y + "width: " + _lineBoxs[0].Width + "Heigh: " + _lineBoxs[0].Height);

                                    Console.WriteLine("Coordinate x of first Para:" + _paraBoxs[0].X + "y: " + _paraBoxs[0].Y + "width: " + _paraBoxs[0].Width + "Heigh: " + _paraBoxs[0].Height);

                                    Console.WriteLine(time_dur);
                                    // Save to property member
                                    ocrDetectedRegion= new Dictionary<SolidColorBrush, List<Rectangle>>();
                                    ocrDetectedRegion.Add(new SolidColorBrush(Colors.Violet), _charBoxs);
                                    ocrDetectedRegion.Add(new SolidColorBrush(Colors.Yellow), _wordBoxs);
                                    ocrDetectedRegion.Add(new SolidColorBrush(Colors.Green), _lineBoxs);
                                    ocrDetectedRegion.Add(new SolidColorBrush(Colors.Red), _paraBoxs);

                                    return resultText;
                                }
                            }
                        }
                    }


                    //textEditor.Text = page.GetText();
                }

            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }

        public Dictionary<SolidColorBrush, List<Rectangle>> GetocrDetectedRegion()
        {
            return this.ocrDetectedRegion;
        }
    }

     
}
