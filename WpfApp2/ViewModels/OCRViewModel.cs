using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp2.Commands;
using WpfApp2.Models;
using System.Collections.ObjectModel;
using WpfApp2.Preprocessor;
using System.ComponentModel;
using WpfApp2;
using WpfApp2.Views;
using System.Drawing;
using System.Windows.Media;
using Newtonsoft.Json;

namespace WpfApp2.ViewModels
{
    /// <summary>
    /// ViewModel of Model-View-ViewModel (MVVM) design pattern
    /// 1. View: as UI
    /// 2. Models folder: contain Data Class
    /// 3. ViewModel: Glue code (Event handling, Binding, logical processing)
    /// </summary>
    public class OCRViewModel : ViewModelBase
    {
        JsonOCRClass jsonObject;
        #region Commands
        public ICommand OpenImageCommand { get; set; }
        public ICommand StartOCRCommand { get; set; }
        public ICommand RGB2GrayCommand { get; set; }
        public ICommand OpenImgInfoCommand { get; set; }
        public ICommand ContrastAdjustCommand { get; set; }

        public ICommand Save2Json { get; set; }

        private Dictionary<SolidColorBrush  , List<Rectangle>> _ocrDetectedRegionVM;

        private ObservableCollection<RectItemClass> _rectItems;
        public ObservableCollection<RectItemClass> RectItems
        {
            get { return _rectItems; }
            set
            {
                _rectItems = value;
                OnPropertyChanged("RectItems");
                Console.WriteLine("update drawing");
            }
        }

        #endregion

        #region pathToTessData
        /// <summary>
        /// This used BackgroundWorker to implement ProgressBar using MVVM model
        /// </summary>
        //private readonly BackgroundWorker worker;
        //private readonly ICommand instigateWorkCommand;
        //public ICommand InstigateWorkCommand
        //{
        //    get
        //    {
        //        return this.instigateWorkCommand;
        //    }
        //}

        private readonly ITesseractOCR _tesseractOCR;
        private readonly string _pathTessData = Environment.CurrentDirectory + @"\tessdata";
        #endregion

        #region Properties
        private List<ImageClass> _imagesList;
        public List<ImageClass> ImagesList
        {
            get { return _imagesList; }
            set
            {
                _imagesList = value;
                OnPropertyChanged("ImagesList");
            }
        }

        private ImageClass _imageOne;
        public ImageClass ImageOne
        {
            get { return _imageOne; }
            set
            {
                _imageOne = value;
                OnPropertyChanged("ImageOne");
            }
        }

        private string _curLang;
        public string CurLang
        {
            get { return _curLang; }
            set
            {
                if (_curLang == value) return;

                _curLang = value;
                OnPropertyChanged("CurLang");
                _tesseractOCR.SelectLang(_curLang);
                //_tesseractOCR.SelectLang("eng");

            }
        }

        private int _loadingBar;
        public int LoadingBar
        {
            get { return _loadingBar; }
            set
            {
                if (this._loadingBar != value)
                {
                    _loadingBar = value;
                    OnPropertyChanged("LoadingBar");
                }
            }
        }

        private ObservableCollection<string> _givenLang;
        public ObservableCollection<string> GivenLang
        {
            get { return _givenLang; }
            set
            {
                if (_givenLang == value) return;
                _givenLang = value;
                OnPropertyChanged("GiveLang");
            }
        }

        #endregion

        private string _outPutText;
        public String OutPutText
        {
            get { return _outPutText; }
            set
            {
                _outPutText = value;
                OnPropertyChanged("OutPutText");
            }
        }

        private string _outTime;
        public String OutTime
        {
            get { return _outTime; }
            set
            {
                _outTime = value;
                OnPropertyChanged("OutTime");
            }
        }

        /// <summary>
        /// RadioBut for select RGB2Gray
        /// </summary>
        /// <param name="ocr"></param>
        /// 

        private Boolean _rgb2grayChecked = false;
        private Boolean _rgb2grayChecked2 = false;
        public Boolean RGB2GrayChecked
        {
            get { return _rgb2grayChecked; }
            set
            {
                if (_rgb2grayChecked == false)
                {
                    _rgb2grayChecked = value;
                    RGB2GrayChecked2 = !(_rgb2grayChecked);
                    OnPropertyChanged("RGB2GrayChecked");

                }
                else
                {
                    _rgb2grayChecked = value;
                    OnPropertyChanged("RGB2GrayChecked");
                }
                Console.WriteLine("_rgb2graychecked111: " + _rgb2grayChecked.ToString());

            }
        }

        public Boolean RGB2GrayChecked2
        {
            get { return _rgb2grayChecked2; }
            set
            {
                if (_rgb2grayChecked2 == false)
                {
                    _rgb2grayChecked2 = value;
                    RGB2GrayChecked = !(_rgb2grayChecked2);
                    OnPropertyChanged("RGB2GrayChecked2");
                }
                else
                {
                    _rgb2grayChecked2 = value;
                    OnPropertyChanged("RGB2GrayChecked2");
                }
                Console.WriteLine("_rgb2graychecked222: " + _rgb2grayChecked2.ToString());
            }
        }

        private Boolean _deskewChecked = false;
        public Boolean DeskewChecked
        {
            get
            {
                return _deskewChecked;
            }
            set
            {
                _deskewChecked = value;
                OnPropertyChanged("DeskewChecked");
                Console.WriteLine("_deskewChecked:" + _deskewChecked);
            }
        }

        /// <summary>
        /// This is used to display region box including (charBox, wordBox, lineBox and ParagraphBox)
        /// </summary>
        #region Option for vivualization of the detected region
        private Boolean _charChecked = false;
        private Boolean _wordChecked = false;
        private Boolean _lineChecked = false;
        private Boolean _paraChecked = false;

        public Boolean CharChecked
        {
            get { return _charChecked; }
            set
            {
                _charChecked = value;
                OnPropertyChanged("CharChecked");
                DrawocrDetectedRegion();
            }
        }

        public Boolean WordChecked
        {
            get { return _wordChecked; }
            set
            {
                _wordChecked = value;
                OnPropertyChanged("WordChecked");
                DrawocrDetectedRegion();
            }
        }

        public Boolean LineChecked
        {
            get { return _lineChecked;}
            set
            {
                _lineChecked = value;
                OnPropertyChanged("LineChecked");
                DrawocrDetectedRegion();
            }
        }

        public Boolean ParaChecked
        {
            get { return _paraChecked; }
            set
            {
                _paraChecked = value;
                OnPropertyChanged("ParaChecked");
                DrawocrDetectedRegion();
            }
        }

        #endregion





        public OCRViewModel(ITesseractOCR ocr)
        {
            _tesseractOCR = ocr;
            //Implement Command based on RelayCommand
            OpenImageCommand = new RelayCommand(OpenImage);
            StartOCRCommand = new RelayCommand(StartOCR);


        }

        public OCRViewModel()
        {
            _tesseractOCR = new TesseractOCR(this);
            OpenImageCommand = new RelayCommand(OpenImage);
            StartOCRCommand = new RelayCommand(StartOCR);
            
            OpenImgInfoCommand = new RelayCommand(OpenImgInfo);
            ContrastAdjustCommand = new RelayCommand(ContrastAdjust);
            Save2Json = new RelayCommand(SaveOutput2Json);
            OutPutText = "Please open an image first";

            GivenLang = new ObservableCollection<string>();
            GivenLang.Add("English");
            GivenLang.Add("Korean");
            GivenLang.Add("Japanese");
            GivenLang.Add("Auto");

            //RectItems = new ObservableCollection<Rectangle>();

            //RectItems.Add(new Rectangle(100, 20, 150, 150));
            //RectItems.Add(new Rectangle(200, 100, 100, 50));
            //RectItems.Add(new Rectangle(20, 40, 70, 30));


            //instigateWorkCommand = new RelayCommand(
            //    o => this.worker.RunWorkerAsync(), o => !this.worker.IsBusy);
            //this.worker = new BackgroundWorker();
            //this.worker.DoWork += this.DoWork;
            //this.worker.ProgressChanged += this.ProgressChanged;
        }

        private void SaveOutput2Json()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "text Document",
                DefaultExt = ".txt",
                Filter = "Text document (.txt)|*.txt"
            };

            string path = dlg.ShowDialog() != true ? null: dlg.FileName;


            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("DO NOT Save");
                return;
            }
            else
            {
                Console.WriteLine("SAVE ok ok");
                using (StreamWriter file = File.CreateText(path))
                {
                    jsonObject = new JsonOCRClass(ImageOne, null, _ocrDetectedRegionVM);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, jsonObject);
                }

            }

        }

        private void ContrastAdjust()
        {
            ContrasAdjust _contrastAdjust = new ContrasAdjust();
            _contrastAdjust.Show();
            Console.WriteLine("Open image Infro");
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.LoadingBar = e.ProgressPercentage;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 20; i++)
            {
                System.Threading.Thread.Sleep(1000);
                OnPropertyChanged("LoadingBar");
            }
        }

        //Window show ImageProperty 
        private void OpenImgInfo()
        {
            if (ImageOne == null)
            {
                MessageBox.Show("Please open at least one image");
                return;
            }

            ShowInfo _showInfo = new ShowInfo(ImageOne);

            _showInfo.Show();
            Console.WriteLine("Open image Infro");
        }

        private void StartOCR()
        {

            Console.WriteLine("Executing StartOCR");
            Console.WriteLine(_pathTessData);
            if (!Directory.Exists(_pathTessData))
            {
                MessageBox.Show("You dont have Tess data. OCR can not Run");
            }
            else
            {
                var tem_Text = _tesseractOCR.OneImageOCR(ImageOne);
                //var tem_Text = "Quan Nguyen Van--->";
                if (tem_Text == null)
                {
                    OutPutText = "No answer";
                    OutTime = _tesseractOCR.getTime() + " ms for running";

                }
                else
                {
                    OutPutText = tem_Text;
                    OutTime = _tesseractOCR.getTime() + " ms for running";
                    _ocrDetectedRegionVM = _tesseractOCR.GetocrDetectedRegion();
                    DrawocrDetectedRegion();
                }
            }


        }
        /// <summary>
        /// This is used to update rectangle box to ObservableCollect and binding to UI
        /// </summary>
        private void DrawocrDetectedRegion()
        {
            RectItems = new ObservableCollection<RectItemClass>();
            if (_ocrDetectedRegionVM!= null)
            {
                foreach(SolidColorBrush colr in _ocrDetectedRegionVM.Keys)
                {
                    //ocrDetectedRegion.Add(new SolidColorBrush(Colors.Violet), _charBoxs);
                    //ocrDetectedRegion.Add(new SolidColorBrush(Colors.Yellow), _wordBoxs);
                    //ocrDetectedRegion.Add(new SolidColorBrush(Colors.Green), _lineBoxs);
                    //ocrDetectedRegion.Add(new SolidColorBrush(Colors.Red), _paraBoxs);
                    if (colr.Color == Colors.Violet && _charChecked == true)
                    {
                        foreach (Rectangle rect in _ocrDetectedRegionVM[colr])
                        {
                            RectItems.Add(new RectItemClass(colr, rect, Visibility.Visible));
                        }

                    }

                    //word
                    if (colr.Color == Colors.Yellow && _wordChecked == true)
                    {
                        foreach (Rectangle rect in _ocrDetectedRegionVM[colr])
                        {
                            RectItems.Add(new RectItemClass(colr, rect, Visibility.Visible));
                        }

                    }
                    //line
                    if (colr.Color == Colors.Green && _lineChecked == true)
                    {
                        foreach (Rectangle rect in _ocrDetectedRegionVM[colr])
                        {
                            RectItems.Add(new RectItemClass(colr, rect, Visibility.Visible));
                        }

                    }
                    //para
                    if (colr.Color == Colors.Red && _paraChecked == true)
                    {
                        foreach (Rectangle rect in _ocrDetectedRegionVM[colr])
                        {
                            RectItems.Add(new RectItemClass(colr, rect, Visibility.Visible));
                        }

                    }

                }
            }
        }

        /// <summary>
        /// This is specificed Command
        /// </summary>
        private void OpenImage()
        {
            //RectItems = new ObservableCollection<RectItemClass>();
            //RectItems.Add(new Rectangle(40, 40, 100, 50));
            //RectItems.Add(new Rectangle(40, 40, 30, 30));

            //Invisible detected reegion of previous image
            CharChecked = false; WordChecked = false;
            LineChecked = false; ParaChecked = false;
            Console.WriteLine("Executing OpenImage");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a image for OCR";
            openFileDialog.Filter = "All supported graphics |*.jpg; *.jpeg;*.png|" +
                "JPEG(*.jpg;*.jpeg)|*.jpg; *.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                //inputImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                ////textEditor.Text = openFileDialog.FileName;
                //textEditor.Text = "Click Run button to see the results";
                //img_Src = openFileDialog.FileName;
                //ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
                //btnRun.IsEnabled = true;
                //toolRun.IsEnabled = true;

                var filename_ = openFileDialog.FileName;
                string filename = null;

                //check the status of Checkbox to perform respective execution

                if (_rgb2grayChecked)
                {
                    filename = RGB2Gray.SaveAndRead(filename_);
                }

                if (_rgb2grayChecked2)
                {
                    filename = ConvertGray.SaveAndRead(filename_);
                }

                if (!_rgb2grayChecked2 && !_rgb2grayChecked)
                {
                    filename = filename_;
                }
                if (_deskewChecked)
                {
                    var tmp = gmseDeskew.SaveAndRead(filename);
                    if (tmp != null)
                    {
                        filename = tmp;
                    }
                }

                var bitmap = new BitmapImage(new Uri(filename));



                ImageOne = new ImageClass
                {
                    FilePath = filename,
                    Image = bitmap
                };

                OutPutText = "Click RUN button to start OCR demo";

            }
        }
    }
}
