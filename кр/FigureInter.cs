using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace кр
{
    class FigureInter
    {
  

        #region Основные классы и конструкторы
        internal class Tuple2<T, TK>
        {
            public T First { get; }
            public TK Second { get; }

            public Tuple2(T first, TK second)
            {
                First = first;
                Second = second;
            }
        }
        internal struct MyPoint
        {
            public float X;
            public float Y;
            public float Constanta;

            public MyPoint(float x = 0.0f, float y = 0.0f, float constanta = 1.0f)
            {
                X = x;
                Y = y;
                Constanta = constanta;
            }

            public Point ToPoint() => new Point((int)X, (int)Y);
        }

        public readonly List<MyPoint> points;
        public List<MyPoint> GetPoints() => points;

        private Graphics Graphics { get; }

        private Pen DrawPenBorder { get; } = new Pen(Color.Orange, 5);
        public FigureInter(Graphics graphics)
        {
            Graphics = graphics;
            points = new List<MyPoint>();
        }

        //конструктор, получающий список точек и область вывода в качестве параметров
        //для фигур
        public FigureInter(List<MyPoint> points, Graphics graphics)
        {
            Graphics = graphics;
            this.points = points;
        }

        //добавление вершин и их соединение линией
        public void AddPoint(MouseEventArgs e, Pen drawPen)
        {
            points.Add(new MyPoint { X = e.X, Y = e.Y });
            if (points.Count > 1)
                Graphics.DrawLine(drawPen, points[points.Count - 2].ToPoint(), points[points.Count - 1].ToPoint());
        }
        #endregion

        #region Вспомогательные методы - заливка и центры для фигур

        #region Для заливки
        //основное условие пересечения границы фигуры со строкой Y
        private bool Check(int i, int k, int y) =>
            (points[i].Y < y && points[k].Y >= y) || (points[i].Y >= y && points[k].Y < y);

        //рассчет точек пересечения, если условие пересечения выполнено
        private List<float> CheckIntersection(List<float> xs, int i, int k, int y)
        {
            if (Check(i, k, y))
            {
                var x = -((y * (points[i].X - points[k].X)) - points[i].X * points[k].Y + points[k].X * points[i].Y)
                        / (points[k].Y - points[i].Y);
                xs.Add(x);
            }
            return xs;
        }

        //отдельный метод для рассчета Ymin и Ymax (самой верхней и самой нижней вершины фигуры)
        public float[] YMinYMax(int height)
        {
            if (points.Count == 0)
                return new float[] { 0, 0, 0 };

            var min = points[0].Y;
            var max = points[0].Y;
            var j = 0;
            for (var i = 0; i < points.Count; i++)
            {
                var item = points[i];
                min = points[i].Y < min ? points[i].Y : min;

                if (item.Y > max)
                {
                    max = item.Y;
                    j = i;
                }
            }

            min = min < 0 ? 0 : min;
            max = max > height ? height : max;
            return new[] { min, max, j };
        }

        public void PaintingLineInFigure(bool haveBorder)
        {
            if (haveBorder && points.Count > 1)
            {
                for (var i = 0; i < points.Count - 1; i++)
                    Graphics.DrawLine(DrawPenBorder, points[i].ToPoint(), points[i + 1].ToPoint());

                Graphics.DrawLine(DrawPenBorder, points[0].ToPoint(), points[points.Count - 1].ToPoint());
            }
        }

        //сама заливка фигуры внутри
        public void FillIn(Pen drawPen, int pictureBoxHeight, bool haveBorder)
        {
            PaintingLineInFigure(haveBorder);
            var arr = YMinYMax(pictureBoxHeight);
            var min = arr[0];
            var max = arr[1];
            var xs = new List<float>();

            for (var y = (int)min; y < max; y++)
            {
                var k = 0;
                for (var i = 0; i < points.Count - 1; i++)
                {
                    k = i < points.Count ? i + 1 : 1;
                    xs = CheckIntersection(xs, i, k, y);
                }

                xs = CheckIntersection(xs, k, 0, y);
                xs.Sort();

                for (var i = 0; i + 1 < xs.Count; i += 2)
                    Graphics.DrawLine(drawPen, new Point((int)xs[i], y), new Point((int)xs[i + 1], y));

                xs.Clear();
            }
        }

        #endregion

      

        //находим центр фигуры
        private MyPoint CenterOfFig(int height)
        {
            float[] Ypoints, Xpoints;
            MyPoint pointFig = new MyPoint();
            Ypoints = YMinYMax(height);
            Xpoints = XminXmax();
            pointFig.X = (Xpoints[0] + Xpoints[1]) / 2 * 0.5f;
            pointFig.Y = (Ypoints[0] + Ypoints[1]) / 2 * 0.5f;
            return pointFig;
        }

        //находим Xmin и Xmax
        private float[] XminXmax()
        {
            var min = points[0].X;
            var max = 0.0f;
            for (var i = 0; i < points.Count; i++)
            {
                min = points[i].X < min ? points[i].X : min;
                max = points[i].X > max ? points[i].X : max;
            }

            return new[] { min, max };
        }

        #endregion

        #region Методы для построения фигур
        //Безье!

        //рассчитываем факториал
        double Factorial(int n)
        {
            double x = 1;
            for (var i = 1; i <= n; i++)
                x *= i;
            return x;
        }

        //формула для полинома
        private double Polinom(int i, int n, float t) => Factorial(n) / (Factorial(i) * Factorial(n - i))
            * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);

        //сам метод рисования безье
        public FigureInter DrawBezier(Pen drPen)
        {
            const double dt = 0.01;
            var t = dt;
            double xPred = points[0].X;
            double yPred = points[0].Y;
            var fig = new List<MyPoint>();
            while (t < 1)
            {
                double x = 0;
                double y = 0;

                for (var i = 0; i < points.Count; i++)
                {
                    var b = Polinom(i, points.Count - 1, (float)t);
                    x += points[i].X * b;
                    y += points[i].Y * b;
                }

                fig.Add(new MyPoint((float)x, (float)y));
                Graphics.DrawLine(drPen, new Point((int)xPred, (int)yPred), new Point((int)x, (int)y));
                t += dt;
                xPred = x;
                yPred = y;
            }

            points.Clear();
            return new FigureInter(fig, Graphics);
        }

        //специальный метод для правильного закрашивания Безье
        public void PaintingBezie(Pen drawPen)
        {
            for (var i = 0; i < points.Count - 1; i++)
                Graphics.DrawLine(drawPen, points[i].ToPoint(), points[i + 1].ToPoint());
        }
        #endregion

        #region Для ТМО
        //поиск левых и правых границ (для метода с ТМО)
        public Tuple2<List<float>, List<float>> CalculationListXrAndXl(int y)
        {

            var k = 0;
            var xR = new List<float>();
            var xL = new List<float>();
            for (var i = 0; i < points.Count - 1; i++)
            {
                k = i < points.Count ? i + 1 : 1;
                if (Check(i, k, y))
                {
                    var x = -((y * (points[i].X - points[k].X))
                                - points[i].X * points[k].Y + points[k].X * points[i].Y)
                            / (points[k].Y - points[i].Y);
                    if (points[k].Y - points[i].Y > 0)
                        xR.Add(x);
                    else
                        xL.Add(x);
                }
            }

            if (Check(k, 0, y))
            {
                var x = -((y * (points[k].X - points[0].X))
                            - points[k].X * points[0].Y + points[0].X * points[k].Y)
                        / (points[0].Y - points[k].Y);
                if (points[0].Y - points[k].Y > 0)
                    xR.Add(x);
                else
                    xL.Add(x);
            }

            return new Tuple2<List<float>, List<float>>(xL, xR);
        }
        #endregion

        #region Методы для геометрических преобразований

        //ГЛАВНЫЙ МЕТОД - МАТРИЦА ПРЕОБРАЗОВАНИЙ
        public static MyPoint MatrixCalc(MyPoint point, float[,] PreobrMatrix) => new MyPoint
        {
            X = point.X * PreobrMatrix[0, 0] + point.Y * PreobrMatrix[1, 0] + point.Constanta * PreobrMatrix[2, 0],
            Y = point.X * PreobrMatrix[0, 1] + point.Y * PreobrMatrix[1, 1] + point.Constanta * PreobrMatrix[2, 1],
            Constanta = point.X * PreobrMatrix[0, 2] + point.Y * PreobrMatrix[1, 2] + point.Constanta * PreobrMatrix[2, 2]
        };

        //масштабирование по ОХ
        public void ChangeSizeX(int height, float[] NewSize)
        {
            if (NewSize[0] <= 0) NewSize[0] = -0.1f;
            if (NewSize[1] <= 0) NewSize[1] = -0.1f;
            if (NewSize[0] >= 0) NewSize[0] = 0.1f;
            if (NewSize[1] >= 0) NewSize[1] = 0.1f;

            var sx = 1 + NewSize[1];
            var sy = 1;
            //матрица преобразования - матрица для масштабирования
            float[,] matrix = {
                {sx,  0, 0 },
                { 0, sy, 0 },
                { 0,  0, 1 }
            };
            var e = CenterOfFig(height);
            Stay(true, e);
            for (int i = 0; i < points.Count; i++)
                points[i] = MatrixCalc(points[i], matrix);
            Stay(false, e);
        }

        //Основной метод для поворота на 45
        public void RotateOnFortyFive(int height, MouseEventArgs em, int operation)
        {
            float certainUg = 45; //градусы
            //перевод градусов в радианы
            double certainUgRADIAN = certainUg * Math.PI / 180;
            var e = operation == 3 ? new MyPoint(em.X, em.Y) : CenterOfFig(height);

            ToAndFromCenter(true, e);

            float[,] matrixRotation =
            {
                {(float) Math.Cos(certainUgRADIAN), (float) Math.Sin(certainUgRADIAN), 0.0f},
                {-(float) Math.Sin(certainUgRADIAN), (float) Math.Cos(certainUgRADIAN), 0.0f},
                {0.0f, 0.0f, 1.0f}
            };
            for (var i = 0; i < points.Count; i++)
            {
                points[i] = MatrixCalc(points[i], matrixRotation);
            }
            ToAndFromCenter(false, e);
        }

        //избежать перемещения фигуры при преобразовании
        private void Stay(bool var1, MyPoint e)
        {
            if (var1)
            {
                float[,] toCenter =
                {
                    {1, 0, 0},
                    {0, 1, 0},
                    {-e.X, -e.Y, 1}
                };
                for (var i = 0; i < points.Count; i++)
                    points[i] = MatrixCalc(points[i], toCenter);
            }
            else
            {
                float[,] fromCenter =
                {
                    {1, 0, 0},
                    {0, 1, 0},
                    {e.X, e.Y, 1}
                };
                for (var i = 0; i < points.Count; i++)
                    points[i] = MatrixCalc(points[i], fromCenter);
            }
        }

        #region Повороты
        private int updateAlpha = 0; //обновляем угол поворота
        //метод поворота на произвольный угол
        public void Rotation(int mouse, int height, TextBox textBox1, MouseEventArgs em, int operation)
        {
            float alpha = 0;
            if (mouse > 0)
            {
                alpha += 0.0175f;
                updateAlpha++;
            }
            else
            {
                alpha -= 0.0175f;
                updateAlpha--;
            }

            textBox1.Text = updateAlpha.ToString();
            var e = operation == 2 ? new MyPoint(em.X, em.Y) : CenterOfFig(height);
            ToAndFromCenter(true, e);

            float[,] matrixRotation =
            {
                {(float) Math.Cos(alpha), (float) Math.Sin(alpha), 0.0f},
                {-(float) Math.Sin(alpha), (float) Math.Cos(alpha), 0.0f},
                {0.0f, 0.0f, 1.0f}
            };
            for (var i = 0; i < points.Count; i++)
                points[i] = MatrixCalc(points[i], matrixRotation);

            ToAndFromCenter(false, e);

        }
        #endregion

        #region Перемещение
        //выбор фигуры
        public bool ThisFigure(int mx, int my)
        {
            var m = 0;
            for (var i = 0; i <= points.Count - 1; i++)
            {
                var k = i < points.Count - 1 ? i + 1 : 0;
                var pi = points[i];
                var pk = points[k];
                if ((pi.Y < my) & (pk.Y >= my) | (pi.Y >= my) & (pk.Y < my)
                    && (my - pi.Y) * (pk.X - pi.X) / (pk.Y - pi.Y) + pi.X < mx)
                    m++;
            }

            return m % 2 == 1;
        }

        //перемещение
        public void Move(int dx, int dy)
        {
            var buffer = new MyPoint();
            for (var i = 0; i <= points.Count - 1; i++)
            {
                buffer.X = points[i].X + dx;
                buffer.Y = points[i].Y + dy;
                points[i] = buffer;
            }
        }

        #endregion

        #endregion


        //сдвиг - в начало координат и произвольный центр
        private void ToAndFromCenter(bool start, MyPoint e)
        {
            if (start)
            {
                float[,] toCenter =
                {
                    {1, 0, 0},
                    {0, 1, 0},
                    {-e.X, -e.Y, 1}
                };
                for (var i = 0; i < points.Count; i++)
                    points[i] = MatrixCalc(points[i], toCenter);
            }
            else
            {
                float[,] fromCenter =
                {
                    {1, 0, 0},
                    {0, 1, 0},
                    {e.X, e.Y, 1}
                };
                for (var i = 0; i < points.Count; i++)
                    points[i] = MatrixCalc(points[i], fromCenter);
            }
        }

        // Клонирование фигуры
        public List<MyPoint> Cloning() => points.ToList();

    }
}
