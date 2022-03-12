using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cvb;

using Emgu.CV.Features2D;
using Emgu.CV.UI;
using Emgu.CV.Util;


namespace ArchitectZad4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        MyImage pic;
        MyImage logo;


        private int captureNumber;
        private bool blnFirstTimeInResizeEvent = true;
        private int intOrigFormWidth, intOrigFormHeight, intOrigImageBoxWidth, intOrigImageBoxHeight;
        private Image<Bgr, Byte> imgSceneColor = null;
        private Image<Bgr, Byte> imgToFindColor = null;
        private Image<Bgr, Byte> imgCopyOfImageToFindWithBorder = null;
        private bool blnImageSceneLoaded = false;
        private bool blnImageToFindLoaded = false;
        private Image<Bgr, Byte> imgResult = null;
        private Bgr bgrKeyPointsColor = new Bgr(Color.Blue);
        private Bgr bgrMatchingLinesColor = new Bgr(Color.LightPink);
        private Bgr bgrFoundImageColor = new Bgr(Color.Red);
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();



        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string a = openFileDialog1.FileName.Replace(@"\", @"\\");
                pictureBox1.Image = Image.FromFile(a);
                pictureBox1.Refresh();

                pic = new MyImage(pictureBox1.Image);

                imgSceneColor = new Image<Bgr, byte>(pic.GetSource());
            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string a = openFileDialog1.FileName.Replace(@"\", @"\\");
                pictureBox2.Image = Image.FromFile(a);
                pictureBox2.Refresh();

                logo = new MyImage(pictureBox2.Image);

                imgToFindColor = new Image<Bgr, byte>(logo.GetSource());
            }
        }


        private void Process()
        {
            SURFDetector surfDetector = new SURFDetector(50, false);
            Image<Gray, Byte> imgSceneGray = null;
            Image<Gray, Byte> imgToFindGray = null;
            VectorOfKeyPoint vkpSceneKeyPoints, vkpToFindKeyPoints;
            Matrix<Single> mtxSceneDescriptors, mtxToFindDescriptors;
            Matrix<Int32> mtxMatchIndices;
            Matrix<Single> mtxDistance;
            Matrix<Byte> mtxMask;
            BruteForceMatcher<Single> bruteForceMatcher;
            HomographyMatrix homographyMatrix = null;
            int intKNumNearestNeighbors = 2;
            double dblUniquenessThreshold = 0.8;
            int intNumNonZeroElements;
            double dblScaleIncrement = 1.5;
            int intRotationBins = 20;
            double dblRansacReprojectionThreshold = 2.0;
            Rectangle rectImageToFind;
            PointF[] ptfPointsF;
            Point[] ptPoints;
            imgSceneGray = imgSceneColor.Convert<Gray, Byte>();
            imgToFindGray = imgToFindColor.Convert<Gray, Byte>();
            vkpSceneKeyPoints = surfDetector.DetectKeyPointsRaw(imgSceneGray, null);


            mtxSceneDescriptors = surfDetector.ComputeDescriptorsRaw(imgSceneGray, null, vkpSceneKeyPoints);
            vkpToFindKeyPoints = surfDetector.DetectKeyPointsRaw(imgToFindGray, null);
            mtxToFindDescriptors = surfDetector.ComputeDescriptorsRaw(imgToFindGray, null, vkpToFindKeyPoints);
            bruteForceMatcher = new BruteForceMatcher<Single>(DistanceType.L2);
            bruteForceMatcher.Add(mtxToFindDescriptors);
            mtxMatchIndices = new Matrix<int>(mtxSceneDescriptors.Rows, intKNumNearestNeighbors);
            mtxDistance = new Matrix<Single>(mtxSceneDescriptors.Rows, intKNumNearestNeighbors);
            bruteForceMatcher.KnnMatch(mtxSceneDescriptors, mtxMatchIndices, mtxDistance, intKNumNearestNeighbors, null);
            mtxMask = new Matrix<Byte>(mtxDistance.Rows, 1);
            mtxMask.SetValue(255);
            Features2DToolbox.VoteForUniqueness(mtxDistance, dblUniquenessThreshold, mtxMask);
            intNumNonZeroElements = CvInvoke.cvCountNonZero(mtxMask);
            if (intNumNonZeroElements >= 16)
            {
                intNumNonZeroElements =
               Features2DToolbox.VoteForSizeAndOrientation(vkpToFindKeyPoints, vkpSceneKeyPoints,
               mtxMatchIndices, mtxMask, dblScaleIncrement, intRotationBins);
                //if (intNumNonZeroElements >= 16)
                {
                    homographyMatrix =
                   Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(vkpToFindKeyPoints,
                   vkpSceneKeyPoints, mtxMatchIndices, mtxMask, dblRansacReprojectionThreshold);
                }
            }
            imgCopyOfImageToFindWithBorder = imgToFindColor.Copy();
            imgCopyOfImageToFindWithBorder.Draw(new Rectangle(1, 1, imgCopyOfImageToFindWithBorder.Width - 3, imgCopyOfImageToFindWithBorder.Height - 3), bgrFoundImageColor, 2);


            if (ckDrawKeyPoints.Checked == true && ckDrawMatchingLines.Checked == true)
                imgResult =
Features2DToolbox.DrawMatches(imgCopyOfImageToFindWithBorder,
 vkpToFindKeyPoints,
 imgSceneColor,
 vkpSceneKeyPoints,
 mtxMatchIndices,
 bgrMatchingLinesColor,
 bgrKeyPointsColor,
 mtxMask,
 Features2DToolbox.KeypointDrawType.DEFAULT);
            else if (ckDrawKeyPoints.Checked == true && ckDrawMatchingLines.Checked == false)
            {
                imgResult = Features2DToolbox.DrawKeypoints(imgSceneColor,
               vkpSceneKeyPoints, bgrKeyPointsColor,
               Features2DToolbox.KeypointDrawType.DEFAULT);
                imgCopyOfImageToFindWithBorder =
               Features2DToolbox.DrawKeypoints(imgCopyOfImageToFindWithBorder,
               vkpToFindKeyPoints, bgrKeyPointsColor,
               Features2DToolbox.KeypointDrawType.DEFAULT);
                imgResult = imgResult.ConcateHorizontal(imgCopyOfImageToFindWithBorder);
            }
            else if (ckDrawKeyPoints.Checked == false && ckDrawMatchingLines.Checked == false)
            {
                imgResult = imgSceneColor;
                imgResult = imgResult.ConcateHorizontal(imgCopyOfImageToFindWithBorder);
            }




            if (homographyMatrix != null)
            {
                rectImageToFind = new Rectangle(0, 0, imgToFindGray.Width,
               imgToFindGray.Height);
                ptfPointsF = new PointF[]
                {
                        new PointF(rectImageToFind.Left, rectImageToFind.Top),
                        new PointF(rectImageToFind.Right, rectImageToFind.Top),
                        new PointF(rectImageToFind.Right, rectImageToFind.Bottom),
                        new PointF(rectImageToFind.Left, rectImageToFind.Bottom)
                };

                homographyMatrix.ProjectPoints(ptfPointsF);
                ptPoints = new Point[]
                {


                         Point.Round(ptfPointsF[0]),
                         Point.Round(ptfPointsF[1]),
                         Point.Round(ptfPointsF[2]),
                         Point.Round(ptfPointsF[3]),
                };
                imgResult.DrawPolyline(ptPoints, true, bgrFoundImageColor, 2);
            }


            imageBox1.Image = imgResult;

        }




        private void button2_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image != null)
            {
                imageBox1.Image.Dispose();
                imageBox1.Image = null;
            }


            if (_cap != null)
            {
                _cap.Dispose();
                _cap = null;
                Application.Idle -= ProcessFrame;

            }


            if (pic != null && logo != null)
            {

                Process();
            }


            //imageBox1.Image = image;
        }

        private void ckDrawMatchingLines_CheckedChanged(object sender, EventArgs e)
        {
            if (ckDrawMatchingLines.Checked)
            {
                ckDrawKeyPoints.Checked = true;
                ckDrawKeyPoints.Enabled = false;
            }
            else
            {
                ckDrawKeyPoints.Enabled = true;
            }
        }

        Capture _cap = null;
        private void button3_Click(object sender, EventArgs e)
        {
            if (_cap != null)
            {
                _cap.Dispose();
                _cap = null;
                Application.Idle -= ProcessFrame;
            }
            else
            {
                if (imageBox1.Image != null)
                {
                    imageBox1.Image.Dispose();
                    imageBox1.Image = null;
                }

                try
                {
                    _cap = new Capture(0);
                    _cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
                    _cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 240);
                    _cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 320);

                    //webcam_frm_cnt = 0;
                    //cam = 1;
                    //Video_seek.Value = 0;
                    Application.Idle += ProcessFrame;
                    //button1.Text = "Stop";

                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }




        }


        private void ProcessFrame(object sender, EventArgs e)
        {
            try
            {
                //Framesno = _cap.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                var frame = _cap.QueryFrame();
                if (frame != null)
                {
                    imgSceneColor = frame;

                    pictureBox1.Image = frame.ToBitmap();

                    Process();

                    //Frame_lbl.Text = "Frame: " + (webcam_frm_cnt++).ToString();
                    frame.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


    }
}
