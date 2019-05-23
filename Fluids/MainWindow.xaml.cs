using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arction.Wpf.Charting;
using Arction.Wpf.Charting.Series3D;
using Arction.Wpf.Charting.Titles;
using Arction.Wpf.Charting.Views.View3D;

namespace Fluids
{
    public partial class MainWindow
    {
        private const int width = 100;
        private const int height = 100;
        private const int maxValue = 100;
        
        private LightningChartUltimate Chart = new LightningChartUltimate();
        
        private SurfaceGridSeries3D grid;
        private SurfaceGridSeries3D ground;
        
        private Stopwatch timer = Stopwatch.StartNew();
        private DispatcherTimer updateTimer = new DispatcherTimer();
        
        private Fluid fluid = new Fluid(width, height);
        
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();

            Width = 1280;
            Height = 720;

            updateTimer.Tick += (o, s) => UpdateFluid();
            UPS_OnValueChanged(null, null);

            setUpTerrain();

            //setUpBorder(20);
            
            SetupGrid();
            
            UpdateGrid();
        }

        private void setUpTerrain()
        {
            var heightmap = new Bitmap("heightmap.png");
            heightmap = new Bitmap(heightmap, 100, 100);
            
            
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    //var h = 20 + Math.Cos(x * 0.1) * Math.Cos(z * 0.2) * 10 + Math.Cos(x * 0.68) * Math.Cos(z * 0.95) * 2;

                    var h = Math.Pow(heightmap.GetPixel(x, z).GetBrightness(), 5) * 100;

                    //var h = 20 + Math.Cos(x * 0.2) * 10 + z * 0.1;
                    
                    fluid.SetGroundHeight(x, z, h);
                }
            }
        }

        private void setUpBorder(double value)
        {
            for (int y = 0; y < height - 1; y++)
            {
                fluid.Add(0, y, value);
                //fluid.Add(width - 1, y, value);
            }

            for (int x = 1; x < width - 1; x++)
            {
                fluid.Add(x, 0, value);
                //fluid.Add(x, height - 1, value);
            }

            //fluid.Add(width - 1, height - 1, value);
        }

        private void UpdateFluid()
        {
            fluid.Update();
            
            UpdateGrid();
        }

        private void SetupGrid()
        {
            (Content as Grid).Children.Insert(0, Chart);
            
            Chart.Title = new ChartTitle {Text = ""};
            Chart.ActiveView = ActiveView.View3D;

            Chart.View3D.YAxisPrimary3D.SetRange(-5, maxValue);
            Chart.View3D.XAxisPrimary3D.SetRange(0, width);
            Chart.View3D.ZAxisPrimary3D.SetRange(0, height);
            Chart.View3D.LegendBox.Visible = false;
            

            grid = new SurfaceGridSeries3D(Chart.View3D,
                Axis3DBinding.Primary,
                Axis3DBinding.Primary,
                Axis3DBinding.Primary)
            {
                RangeMinX = 0, RangeMinZ = 0, RangeMaxX = width, RangeMaxZ = height,
                SizeX = width, SizeZ = height,
                ColorSaturation = 50, ToneColor = Colors.SkyBlue,
                Fill = SurfaceFillStyle.Toned
            };
            

            Chart.View3D.SurfaceGridSeries3D.Add(grid);
            
            ground = new SurfaceGridSeries3D(Chart.View3D,
                Axis3DBinding.Primary,
                Axis3DBinding.Primary,
                Axis3DBinding.Primary)
            {
                RangeMinX = 0, RangeMinZ = 0, RangeMaxX = width, RangeMaxZ = height,
                SizeX = width, SizeZ = height,
                ColorSaturation = 25, ToneColor = Colors.DimGray,
                Fill = SurfaceFillStyle.Toned
            };
            
            Chart.View3D.SurfaceGridSeries3D.Add(ground);
        }

        public void UpdateGrid()
        {
            Chart.BeginUpdate();

            var t = timer.Elapsed.TotalSeconds;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    grid.Data[x, z].Y = fluid.WaterLevelAt(x, z);
                    ground.Data[x, z].Y = fluid.GroundLevelAt(x, z);
                }
            }
            
            grid.InvalidateData();
            ground.InvalidateData();
            
            Chart.EndUpdate();
        }

        private void AutoUpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            updateTimer.IsEnabled = AutoUpdateButton.IsChecked ?? false;
        }

        private void StepButton_OnClick(object sender, RoutedEventArgs e)
        {
            AutoUpdateButton.IsChecked = false;
            updateTimer.Stop();
            
            UpdateFluid();
        }

        private void AddToCenter_OnClick(object sender, RoutedEventArgs e)
        {
            add(width / 2, height / 2);
        }

        private void UPS_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateTimer.Interval = TimeSpan.FromSeconds(1 / UPS.Value);
        }

        private void AddAtRandom_OnClick(object sender, RoutedEventArgs e)
        {
            add(random.Next(width), random.Next(height));
        }

        private void add(int x, int y)
        {
            fluid.Add(x, y, Math.Pow(10, AddValueExponent.Value));
            UpdateGrid();
        }
    }
}
