using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UR1_Project
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        private int _BinaryThreshold = 150;//next 6 lines are what initialized the changeable screen
        private int _HLowerThreshold = 17;
        private int _HUpperThreshold = 33;
        private int _SLowerThreshold = 93;
        private int _SUpperThreshold = 255;
        private int _VLowerThreshold = 151;
        private int _VUpperThreshold = 255;
        private char _LastAction = 'b';
        private Robot _robot = new Robot("COM13");//find com port

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0);
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
            BinaryTrackBar.Value = _BinaryThreshold;//sets the track bar values to the initialized values 
            HLower.Value = _HLowerThreshold;
            HUpper.Value = _HUpperThreshold;
            SLower.Value = _SLowerThreshold;
            SUpper.Value = _SUpperThreshold;
            VLower.Value = _VLowerThreshold;
            VUpper.Value = _VUpperThreshold;
        }

        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                //Sets up the initial Raw Image, Binary Image and Splits HSV frame////////////////////////////////////////////
                Mat frame = _capture.QueryFrame();

                int newHeight = (frame.Size.Height * NormalBox.Size.Width) / frame.Size.Width;
                Size newSize = new Size(NormalBox.Size.Width, newHeight);
                CvInvoke.Resize(frame, frame, newSize);

                NormalBox.Image = frame.Bitmap;

                Mat grayFrame = new Mat();
                Mat binaryFrame = new Mat();
                Mat hsvFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, binaryFrame, _BinaryThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                CvInvoke.CvtColor(frame, hsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

                BinaryPictureBox.Image = binaryFrame.Bitmap;
                Mat[] hsvChannels = hsvFrame.Split();
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //changeable Box////////////////////////////////////////////////////////////////////////////////////////
                Mat hueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(_HLowerThreshold), new ScalarArray(_HUpperThreshold), hueFilter);
                Invoke(new Action(() => { HBox.Image = hueFilter.Bitmap; }));

                Mat saturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(_SLowerThreshold), new ScalarArray(_SUpperThreshold), saturationFilter);
                Invoke(new Action(() => { SBox.Image = saturationFilter.Bitmap; }));

                Mat valueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(_VLowerThreshold), new ScalarArray(_VUpperThreshold), valueFilter);
                Invoke(new Action(() => { VBox.Image = valueFilter.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //Yellow Box////////////////////////////////////////////////////////////////////////////////////////////////
                Mat YhueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(17), new ScalarArray(33), YhueFilter);
                Invoke(new Action(() => { HBox.Image = YhueFilter.Bitmap; }));

                Mat YsaturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(93), new ScalarArray(255), YsaturationFilter);
                Invoke(new Action(() => { SBox.Image = YsaturationFilter.Bitmap; }));

                Mat YvalueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(151), new ScalarArray(255), YvalueFilter);
                Invoke(new Action(() => { VBox.Image = YvalueFilter.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //Red Box////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Mat RhueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(129), new ScalarArray(179), RhueFilter);
                Invoke(new Action(() => { HBox.Image = RhueFilter.Bitmap; }));

                Mat RsaturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(0), new ScalarArray(255), RsaturationFilter);
                Invoke(new Action(() => { SBox.Image = RsaturationFilter.Bitmap; }));

                Mat RvalueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(100), new ScalarArray(255), RvalueFilter);
                Invoke(new Action(() => { VBox.Image = RvalueFilter.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //Merged for changeable////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Mat mergedImage = new Mat();
                CvInvoke.BitwiseAnd(hueFilter, saturationFilter, mergedImage);
                CvInvoke.BitwiseAnd(mergedImage, valueFilter, mergedImage);
                Invoke(new Action(() => { CombinationBox.Image = mergedImage.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //Merged for Yellow////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Mat YmergedImage = new Mat();
                CvInvoke.BitwiseAnd(YhueFilter, YsaturationFilter, YmergedImage);
                CvInvoke.BitwiseAnd(YmergedImage, YvalueFilter, YmergedImage);
                Invoke(new Action(() => { YellowBox.Image = YmergedImage.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //Merged for Red////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Mat RmergedImage = new Mat();
                CvInvoke.BitwiseAnd(RhueFilter, RsaturationFilter, RmergedImage);
                CvInvoke.BitwiseAnd(RmergedImage, RvalueFilter, RmergedImage);
                Invoke(new Action(() => { RedBox.Image = RmergedImage.Bitmap; }));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //White
                //Counts the White Pixels on the screen in sevenths
                int WPLLL = 0, WPLL = 0, WPL = 0, WPM = 0, WPR = 0, WPRR = 0, WPRRR = 0; //counters for the binary frame of sevenths 
                int sliceWidth = frame.Width / 7; //easier syntax for the sevenths
                Image<Gray, byte> Bimg = binaryFrame.ToImage<Gray, byte>();
                for (int y = 0; y < binaryFrame.Height; y++)
                {
                    //first seventh
                    for (int x = 0; x < sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPLLL++;
                        }
                    }

                    //second seventh
                    for (int x = sliceWidth; x < 2 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPLL++;
                        }
                    }

                    //third seventh
                    for (int x = 2 * sliceWidth; x < 3 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPL++;
                        }
                    }

                    //fourth seventh
                    for (int x = 3 * sliceWidth; x < 4 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPM++;
                        }
                    }

                    //fifth seventh
                    for (int x = 4 * sliceWidth; x < 5 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPR++;
                        }
                    }

                    //first seventh
                    for (int x = 5 * sliceWidth; x < 6 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPRR++;
                        }
                    }

                    //first seventh
                    for (int x = 6 * sliceWidth; x < 7 * sliceWidth; x++)
                    {
                        if (Bimg.Data[y, x, 0] == 255)
                        {
                            WPRRR++;
                        }
                    }
                }

                //writing the White Pixel Values to the Screen
                Invoke(new Action(() =>
                {
                    column1.Text = $"{WPLLL}";
                    column2.Text = $"{WPLL}";
                    column3.Text = $"{WPL}";
                    column4.Text = $"{WPM}";
                    column5.Text = $"{WPR}";
                    column6.Text = $"{WPRR}";
                    column7.Text = $"{WPRRR}";
                }));
                
                //counts the Pixels on the merged yellow and red screens 
                int MIP = 0, YIP = 0, RIP = 0; //merged yellow and red pixels counters 
                Image<Gray, byte> Mimg = mergedImage.ToImage<Gray, byte>();
                Image<Gray, byte> Yimg = YmergedImage.ToImage<Gray, byte>();
                Image<Gray, byte> Rimg = RmergedImage.ToImage<Gray, byte>();
                for (int y = 0; y < mergedImage.Height; y++)//all can use mergedImage.Height and Width because all boxes the same size
                {
                    for (int x = 0; x < mergedImage.Width; x++)
                    {
                        //Merged counter
                        if (Mimg.Data[y, x, 0] == 255)
                        {
                            MIP++;
                        }

                        //Yellow counter
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YIP++;
                        }

                        //Red counter
                        if (Rimg.Data[y, x, 0] == 255)
                        {
                            RIP++;
                        }
                    }
                }


                //Yellow
                //Counts the Yellow Pixels on the screen in sevenths
                int YPLLL = 0, YPLL = 0, YPL = 0, YPM = 0, YPR = 0, YPRR = 0, YPRRR = 0; //counters for the yellow frame of sevenths
                for (int y = 0; y < binaryFrame.Height; y++)
                {
                    //first seventh
                    for (int x = 0; x < sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPLLL++;
                        }
                    }

                    //second seventh
                    for (int x = sliceWidth; x < 2 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPLL++;
                        }
                    }

                    //third seventh
                    for (int x = 2 * sliceWidth; x < 3 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPL++;
                        }
                    }

                    //fourth seventh
                    for (int x = 3 * sliceWidth; x < 4 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPM++;
                        }
                    }

                    //fifth seventh
                    for (int x = 4 * sliceWidth; x < 5 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPR++;
                        }
                    }

                    //first seventh
                    for (int x = 5 * sliceWidth; x < 6 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPRR++;
                        }
                    }

                    //first seventh
                    for (int x = 6 * sliceWidth; x < 7 * sliceWidth; x++)
                    {
                        if (Yimg.Data[y, x, 0] == 255)
                        {
                            YPRRR++;
                        }
                    }
                }

                //writing the Yellow Pixel Values to the Screen
                Invoke(new Action(() =>
                {
                    ycolumn1.Text = $"{YPLLL}";
                    ycolumn2.Text = $"{YPLL}";
                    ycolumn3.Text = $"{YPL}";
                    ycolumn4.Text = $"{YPM}";
                    ycolumn5.Text = $"{YPR}";
                    ycolumn6.Text = $"{YPRR}";
                    ycolumn7.Text = $"{YPRRR}";
                }));

                //writes the Total Pixel Values to the Screen
                Invoke(new Action(() =>
                {
                    CombinationPixels.Text = $"{MIP}";
                    YellowPixels.Text = $"{YIP}";
                    RedPixels.Text = $"{RIP}";
                }));

                //Writes the TrackBar Values to the screen
                Invoke(new Action(() =>
                {
                    trackbar.Text = $"{_BinaryThreshold}";
                    HL.Text = $"{_HLowerThreshold}";
                    HU.Text = $"{_HUpperThreshold}";
                    SL.Text = $"{_SLowerThreshold}";
                    SU.Text = $"{_SUpperThreshold}";
                    VL.Text = $"{_VLowerThreshold}";
                    VU.Text = $"{_VUpperThreshold}";
                }));

                ///////Sending Logic to arduino///////////////////////////////////////////////////
                if(RIP > 4000)
                {
                    _robot.Move(Robot.STOP);
                    //return 0
                    //stop
                }
                else if(YPM == 0 && YPR == YPL && YPRR == YPLL)
                {
                    _robot.Move(Robot.LAST_ACTION);
                }
                else if(YPM > 0 && YPR == YPL && YPRR == YPLL)
                {
                    _robot.Move(Robot.FORWARD);
                    //return 1
                    //straight 
                }    
                else if (YPM >= 0 && YPR > 2*YPL && YPRR >= YPLL)
                {
                    _robot.Move(Robot.HARD_RIGHT);
                    //return 4
                    //hard right 
                }
                else if (YPM >= 0 && YPR > 2*YPL && YPRR >= YPLL)
                {
                    _robot.Move(Robot.HARD_LEFT);
                    //return 5
                    //hard left
                }
                else if (YPM >= 0 && YPR > YPL && YPRR >= YPLL)
                {
                    _robot.Move(Robot.SLIGHT_RIGHT);
                    //return 2
                    //slight right 
                }
                else if (YPM >= 0 && YPR < YPL && YPRR <= YPLL)
                {
                    _robot.Move(Robot.SLIGHT_LEFT   );
                    //return 3
                    //slight left
                }

                             //Thread.Sleep(100);
                ///////////////////////////////////////////////////////////////////////
            }
            
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }

        private void BinaryTrackBar_Scroll(object sender, EventArgs e)
        {
            _BinaryThreshold = BinaryTrackBar.Value;
        }

        private void HLower_Scroll(object sender, EventArgs e)
        {
            _HLowerThreshold = HLower.Value;
        }

        private void HUpper_Scroll(object sender, EventArgs e)
        {
            _HUpperThreshold = HUpper.Value;
        }

        private void SLower_Scroll(object sender, EventArgs e)
        {
            _SLowerThreshold = SLower.Value;
        }

        private void SUpper_Scroll(object sender, EventArgs e)
        {
            _SUpperThreshold = SUpper.Value;
        }

        private void VLower_Scroll(object sender, EventArgs e)
        {
            _VLowerThreshold = VLower.Value;
        }

        private void VUpper_Scroll(object sender, EventArgs e)
        {
            _VUpperThreshold = VUpper.Value;
        }

  
    }
}
