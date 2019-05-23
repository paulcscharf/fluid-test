
using System;

namespace Fluids
{
    public class Fluid
    {
        private readonly int width;
        private readonly int height;

        private readonly double[,] groundHeight;
        private readonly double[,] volume;
        private double[,] flowX;
        private double[,] flowY;
        private double[,] flowXPrevious;
        private double[,] flowYPrevious;

        public Fluid(int width, int height)
        {
            this.width = width;
            this.height = height;
            groundHeight = new double[width, height];
            volume = new double[width, height];
            flowX = new double[width - 1, height - 1];
            flowY = new double[width - 1, height - 1];
            flowXPrevious = new double[width - 1, height - 1];
            flowYPrevious = new double[width - 1, height - 1];
        }

        public void Add(int x, int y, double amount)
        {
            volume[x, y] += amount;
        }

        public void SetGroundHeight(int x, int y, double height)
        {
            groundHeight[x, y] = height;
        }

        public double WaterLevelAt(int x, int y) => waterLevel(x, y);
        public double GroundLevelAt(int x, int y) => groundHeight[x, y];

        public void Update()
        {
            updateFlow();
            spreadFlow();
            dampenFlow();
            applyFlow();
        }

        private void spreadFlow()
        {
            var spreadAmount = 0.2;
            var inverseSpreadAmount = 1 - spreadAmount;
            
            (flowXPrevious, flowX) = (flowX, flowXPrevious);
            (flowYPrevious, flowY) = (flowY, flowYPrevious);

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    flowX[x, y] = flowXPrevious[x, y] * inverseSpreadAmount;
                    flowY[x, y] = flowYPrevious[x, y] * inverseSpreadAmount;
                }
            }
            
            for (int y = 1; y < height - 2; y++)
            {
                for (int x = 1; x < width - 2; x++)
                {
                    var fx = flowXPrevious[x, y];

                    if (fx > 0)
                    {
                        flowX[x + 1, y] += fx * spreadAmount;
                    }
                    else
                    {
                        flowX[x - 1, y] += fx * spreadAmount;
                    }
                    
                    var fy = flowYPrevious[x, y];
                    
                    if (fy > 0)
                    {
                        flowY[x, y + 1] += fy * spreadAmount;
                    }
                    else
                    {
                        flowY[x, y - 1] += fy * spreadAmount;
                    }
                }
            }
        }

        private void updateFlow()
        {
            var viscosity = 3;
            var inverseViscosity = 0.25 / viscosity;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var deltaX = waterLevel(x, y) - waterLevel(x + 1, y);

                    flowX[x, y] += deltaX * inverseViscosity;
                    
                    
                    var deltaY =  waterLevel(x, y) - waterLevel(x, y + 1);

                    flowY[x, y] += deltaY * inverseViscosity;
                }
            }
        }

        private double waterLevel(int x, int y)
        {
            var v = volume[x, y];
            return groundHeight[x, y] + v;
        }

        private void dampenFlow()
        {
            const bool minimumFlow = false;
            var damping = 0.99;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var fx = flowX[x, y];
                    var fy = flowY[x, y];

                    if (minimumFlow && Math.Abs(fx) < 0.1)
                    {
                        fx = 0;
                    }
                    else if (fx > 0)
                    {
                        fx = Math.Min(fx, volume[x, y] / 4);
                    }
                    else
                    {
                        fx = -Math.Min(-fx, volume[x + 1, y] / 4);
                    }
                    
                    if (minimumFlow && Math.Abs(fy) < 0.1)
                    {
                        fy = 0;
                    }
                    else if (fy > 0)
                    {
                        fy = Math.Min(fy, volume[x, y] / 4);
                    }
                    else
                    {
                        fy = -Math.Min(-fy, volume[x, y + 1] / 4);
                    }

                    fx *= damping;
                    fy *= damping;
                    
                    flowX[x, y] = fx;
                    flowY[x, y] = fy;
                }
            }
        }

        private void applyFlow()
        {
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    var previousVolume = volume[x, y];

                    var deltaX = flowX[x - 1, y] - flowX[x, y];
                    var deltaY = flowY[x, y - 1] - flowY[x, y];

                    var newVolume = previousVolume + deltaX + deltaY;

                    volume[x, y] = newVolume;
                }
            }
        }
    }
}
