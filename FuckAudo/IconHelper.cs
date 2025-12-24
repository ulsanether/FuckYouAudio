using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace FuckAuido
{
    /// <summary>
    /// 아이콘 생성 헬퍼 클래스
    /// </summary>
    public static class IconHelper
    {
        /// <summary>
        /// 볼륨 아이콘 생성 (시스템 트레이용)
        /// </summary>
        public static Icon CreateVolumeIcon(int volume = 100, bool isMuted = false)
        {
            int size = 256;
            Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 스피커 모양 그리기
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    // 스피커 베이스
                    Point[] speakerPoints = new Point[]
                    {
                        new Point(40, 90),
                        new Point(90, 90),
                        new Point(150, 40),
                        new Point(150, 216),
                        new Point(90, 166),
                        new Point(40, 166)
                    };
                    g.FillPolygon(brush, speakerPoints);

                    // 스피커 그릴
                    Rectangle speakerGrill = new Rectangle(30, 100, 40, 56);
                    g.FillRectangle(brush, speakerGrill);
                }

                if (isMuted)
                {
                    // 음소거 X 표시
                    using (Pen pen = new Pen(Color.Red, 20))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g.DrawLine(pen, 170, 80, 230, 176);
                        g.DrawLine(pen, 230, 80, 170, 176);
                    }
                }
                else
                {
                    // 사운드 웨이브
                    using (Pen pen = new Pen(Color.White, 12))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;

                        if (volume > 0)
                        {
                            // 첫 번째 웨이브 (항상 표시)
                            g.DrawArc(pen, 160, 100, 40, 56, -50, 100);
                        }

                        if (volume > 33)
                        {
                            // 두 번째 웨이브
                            g.DrawArc(pen, 175, 80, 70, 96, -50, 100);
                        }

                        if (volume > 66)
                        {
                            // 세 번째 웨이브
                            g.DrawArc(pen, 190, 60, 100, 136, -50, 100);
                        }
                    }
                }
            }

            // Bitmap을 Icon으로 변환
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }

        /// <summary>
        /// 볼륨 아이콘을 파일로 저장
        /// </summary>
        public static void SaveVolumeIconToFile(string filePath, int volume = 100, bool isMuted = false)
        {
            using (Icon icon = CreateVolumeIcon(volume, isMuted))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    icon.Save(fs);
                }
            }
        }

        /// <summary>
        /// 설정 아이콘 생성
        /// </summary>
        public static Icon CreateSettingsIcon()
        {
            int size = 256;
            Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (SolidBrush brush = new SolidBrush(Color.White))
                using (Pen pen = new Pen(Color.White, 12))
                {
                    // 기어 모양 그리기
                    Point center = new Point(128, 128);
                    int innerRadius = 40;
                    int outerRadius = 90;
                    int teeth = 8;

                    PointF[] gearPoints = new PointF[teeth * 4];

                    for (int i = 0; i < teeth; i++)
                    {
                        double angle1 = (i * 360.0 / teeth - 10) * Math.PI / 180;
                        double angle2 = (i * 360.0 / teeth) * Math.PI / 180;
                        double angle3 = (i * 360.0 / teeth + 10) * Math.PI / 180;
                        double angle4 = ((i + 1) * 360.0 / teeth - 10) * Math.PI / 180;

                        gearPoints[i * 4] = new PointF(
                            (float)(center.X + innerRadius * Math.Cos(angle1)),
                            (float)(center.Y + innerRadius * Math.Sin(angle1))
                        );
                        gearPoints[i * 4 + 1] = new PointF(
                            (float)(center.X + outerRadius * Math.Cos(angle2)),
                            (float)(center.Y + outerRadius * Math.Sin(angle2))
                        );
                        gearPoints[i * 4 + 2] = new PointF(
                            (float)(center.X + outerRadius * Math.Cos(angle3)),
                            (float)(center.Y + outerRadius * Math.Sin(angle3))
                        );
                        gearPoints[i * 4 + 3] = new PointF(
                            (float)(center.X + innerRadius * Math.Cos(angle4)),
                            (float)(center.Y + innerRadius * Math.Sin(angle4))
                        );
                    }

                    g.FillPolygon(brush, gearPoints);

                    // 중앙 원
                    g.FillEllipse(Brushes.Black, 88, 88, 80, 80);
                    g.FillEllipse(brush, 98, 98, 60, 60);
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }

        /// <summary>
        /// BitmapImage를 Icon으로 변환
        /// </summary>
        public static Icon BitmapImageToIcon(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);

                Bitmap bitmap = new Bitmap(outStream);
                IntPtr hIcon = bitmap.GetHicon();
                Icon icon = Icon.FromHandle(hIcon);

                return icon;
            }
        }
    }
}
