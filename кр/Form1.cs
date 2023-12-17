using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace кр
{
    public partial class Form1 : Form
    {
        //здесь будет перечисление фигур
        //это множество (enum)
        private enum Figures
        {
            Colba,
            Star,
            Bezie,
        }

        //объект типа bitmap
        //в котором будет храниться созданная картинка
        Bitmap picture;
        //объект типа Grafics для рисования изображения
        Graphics g;

        //объект из множества фигур
        //для будущего выбора, какую именно рисовать
        private Figures figure;
        //список фигур
        private readonly List<FigureInter> listFigure = new List<FigureInter>();
        //инициализация объекта типа класса FigureInter
        private FigureInter fig;

        #region Переменные для фигур
        //переменная для количества углов у звезды
        int angles;
        //--для безье
        const int np = 20;
        Point[] ArPoints = new Point[np];
        private bool Bezie;
        int BeginParams = 0;
        #endregion

        #region Объекты типа "Pen"
        Pen DrawPen;
        Pen p1;
        Pen p2;
        Pen DrawPenWhite = new Pen(Color.White, 1); //эта ручка красит белым для ТМО
        #endregion

        List<Point> VertexList1 = new List<Point>(); //список точек для первой фигуры
        List<Point> VertexList2 = new List<Point>(); //список точек для второй фигуры
        List<Point> Star = new List<Point>(); //список точек для звезды

        #region Переменные и класс для ТМО
        int TMO = 1; //начальный код для тмо - все тмо кодируются от 1 до 5. Он же - параметр алгоритма
        int[] SetQ = new int[2]; //множество значений суммы Q пороговых функций
        bool IsTMO = true;
        private M[] arrayM; //рабочий массив

        //класс для массива М
        private class M
        {
            public float X { get; }
            public int Dq { get; }

            public M(float x, int dQ)
            {
                X = x;
                Dq = dQ;
            }
        }
        #endregion


        bool FigureIsClicked = false; //выбрана ли фигура
        private Point pictureBoxMousePosition; //ищет позицию мыши
        private bool haveBorder = false;
        private int OperationOnClick = 0; //операции над фигурами
        private int Geometric = 0; //масштабирование по Х
        private bool FortyFiveRotate = false; //поворот на 45
        bool tmo = false;
        public Form1()

        {
            InitializeComponent();
            //--смена курсора
           // pictureBox1.Cursor = new Cursor("BrushCur.cur");
            //---

            //размер картинки такой же, как у формы для рисования
            picture = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(picture);
            fig = new FigureInter(g);
            //более плавное отображение линий
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            DrawPen = new Pen(Color.Black, 1); //по умолчанию
            p1 = new Pen(Color.Black, 2); //отрисовка границ первой фигуры
            p2 = new Pen(Color.BurlyWood, 2); //отрисовка границ второй фигур

            #region Новые события для колесика мышки
            MouseWheel += new MouseEventHandler(ChangeSizeX);
            MouseWheel += new MouseEventHandler(CertainRotate);
            MouseWheel += new MouseEventHandler(RotatePrandDeg);
            #endregion
        }


        #region Выбор фигур по кнопкам и присвоение им кодов операций
        //выбор безье
        private void button5_Click(object sender, EventArgs e)
        {
            figure = Figures.Bezie; //выбираем из множества и присваиваем объекту
            OperationOnClick = 1;
            Bezie = true;
        }

        //выбор звезды
        private void button6_Click(object sender, EventArgs e)
        {
            figure = Figures.Star;
            OperationOnClick = 2;
            Bezie = false;
        }

        //выбор Колбы
        private void button7_Click(object sender, EventArgs e)
        {
            figure = Figures.Colba;
            OperationOnClick = 3;
            Bezie = false;
        }


        #endregion Выбор фигур


        //-ОСНОВНОЙ ОБРАБОТЧИК СОБЫТИЙ
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBoxMousePosition = e.Location;
            if ((figure == Figures.Bezie) && (OperationOnClick == 1))
            {
                if ((e.Button == MouseButtons.Right) && Bezie)
                {
                    fig = fig.DrawBezier(DrawPen);
                    listFigure.Add(CloningFigure());
                    fig.GetPoints().Clear();
                }
                else
                {
                    fig.AddPoint(e, DrawPen);
                }

            }
            else
            {
                if ((figure == Figures.Star) && (OperationOnClick == 2))
                {
                    CreateStar(e);
                    fig.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                    listFigure.Add(CloningFigure());
                    fig.GetPoints().Clear();
                }
                else
                {
                    if ((figure == Figures.Colba) && (OperationOnClick == 3))
                    {
                        var colbochka = new List<FigureInter.MyPoint>() //создаем объект - множество вершин
                            {
                                new FigureInter.MyPoint(e.X, e.Y), //координаты с клика мыши
                                new FigureInter.MyPoint(e.X - 60, e.Y),
                                new FigureInter.MyPoint(e.X - 40, e.Y - 30),
                                new FigureInter.MyPoint(e.X - 40, e.Y - 60),
                                new FigureInter.MyPoint(e.X - 20, e.Y - 60),
                                new FigureInter.MyPoint(e.X - 20, e.Y - 30),
                                new FigureInter.MyPoint(e.X, e.Y), //возвращаемся в исходную
                            };
                        fig = new FigureInter(colbochka, g); //создание новой фигуры - объект "колба"
                        fig.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                        listFigure.Add(CloningFigure());
                        fig.GetPoints().Clear();
                    }
                    if (OperationOnClick == 5)
                    {
                        ThisFugure(e);
                    }
                    if (OperationOnClick == 6) //произвольный поворот
                    {
                        listFigure[listFigure.Count - 1].Rotation(e.Delta, pictureBox1.Height, textBox1, e, Geometric);

                    }
                    if (OperationOnClick == 7) //поворот на 45 градусов
                    {
                        if ((FortyFiveRotate) && (FigureIsClicked))
                        {
                            listFigure[listFigure.Count - 1].RotateOnFortyFive(pictureBox1.Height, e, Geometric);
                            fig.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                        }
                    }
                }

            }
            pictureBox1.Image = picture;
        }


        //------
        #region Методы, необходимые для построения фигур




        #region Звезда
        //выбор количества углов пользователем для звезды
        //+4 - т.к. индексирование идет с 0
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            angles = comboBox1.SelectedIndex + 4;
        }

        //звезда
        private void CreateStar(MouseEventArgs e)
        {
            const double R = 25; // радиус внутренний
            const double r = 50; // радиус внешний
            const double d = 0; // поворот
            double a = d, da = Math.PI / angles, l;
            var star = new List<FigureInter.MyPoint>();
            for (var k = 0; k < 2 * angles + 1; k++)
            {
                l = k % 2 == 0 ? r : R;
                star.Add(new FigureInter.MyPoint((int)(e.X + l * Math.Cos(a)), (int)(e.Y + l * Math.Sin(a))));
                a += da;
            }

            fig = new FigureInter(star, g);

        }
        #endregion
        #endregion

        //добавление фигуры в список фигур
        private FigureInter CloningFigure() => new FigureInter(fig.Cloning(), g);

        #region Методы для ТМО
        public struct SpecialArray
        {
            public int x; //х - координата границы сегмента
            public int dQ; //приращение пороговой функции с учетом веса операнда
            public SpecialArray(int _x, int _dQ)
            {
                this.x = _x;
                this.dQ = _dQ;
            }
        }

        //обработчик нажатия на кнопку "Применить ТМО"
        private void button9_Click(object sender, EventArgs e)
        {
            //вызываем проверку
            CheckWorkTmo();
            //сохраняе результат ТМО
            pictureBox1.Image = picture;
        }

        //основной метод для осуществления ТМО
        //проверяется, выполнены ли все условия для выполнения ТМО
        private void CheckWorkTmo()
        {
            if ((listFigure.Count > 1) && (IsTMO))
            {
                g.Clear(Color.White);
                listFigure[0].PaintingLineInFigure(haveBorder); // Рисуем стороны первой фигуры
                listFigure[1].PaintingLineInFigure(haveBorder); // Рисуем стороны второй фигуры
                TMORealization(); //выполнение ТМО
            }
            else
            {
                MessageBox.Show("Нужно две фигуры! Дорисуйте еще одну!", "Ошибка: недостаточно фигур для ТМО",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
            fig.GetPoints().Clear();
        }


        #region кодирование операций ТМО
        //кодирование операций ТМО
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex) 
            {
                case 0:
                    TMO = 3;
                    SetQ[0] = 1;
                    SetQ[1] = 2;
                    break;
                case 1:
                    TMO = 4;
                    SetQ[0] = 2;
                    SetQ[1] = 2;
                    break;
                case 2:
                    TMO = 5;
                    SetQ[0] = 1;
                    SetQ[1] = 1;
                    break;
                case 3:
                    TMO = 2;
                    SetQ[0] = 3;
                    SetQ[1] = 3;
                    break;
                case 4:
                    TMO = 1;
                    SetQ[0] = 1;
                    SetQ[1] = 3;
                    break;
            }
        }
        #endregion





        //реализация ТМО
        private void TMORealization()
        {
            var arrFirstFig = listFigure[0].YMinYMax(pictureBox1.Height);
            var arrSecondFig = listFigure[1].YMinYMax(pictureBox1.Height);
            var Ymin = arrFirstFig[0] < arrSecondFig[0] ? arrFirstFig[0] : arrSecondFig[0];
            var Ymax = arrFirstFig[1] > arrSecondFig[1] ? arrFirstFig[1] : arrSecondFig[1];
            for (var Y = (int)Ymin; Y < Ymax; Y++)
            {
                //находим все координаты Х для левых и правых границ ПЕРВОЙ фигуры
                var FigA = listFigure[0].CalculationListXrAndXl(Y);
                List<float> xFigAleft = FigA.First;
                List<float> xFigAright = FigA.Second;
                //находим все координаты Х для левых и правых границ ВТОРОЙ фигуры
                var FigB = listFigure[1].CalculationListXrAndXl(Y);
                List<float> xFigBleft = FigB.First;
                List<float> xFigBright = FigB.Second;
                if (xFigAleft.Count == 0 && xFigBleft.Count == 0)
                    continue;

                //заполняем рабочий массив М
                arrayM = new M[xFigAleft.Count * 2 + xFigBleft.Count * 2];
                for (int i = 0; i < xFigAleft.Count; i++)
                    arrayM[i] = new M(xFigAleft[i], 2);

                var nM = xFigAleft.Count;
                for (int i = 0; i < xFigAright.Count; i++)
                    arrayM[nM + i] = new M(xFigAright[i], -2);

                nM += xFigAright.Count;
                for (int i = 0; i < xFigBleft.Count; i++)
                    arrayM[nM + i] = new M(xFigBleft[i], 1);

                nM += xFigBleft.Count;
                for (int i = 0; i < xFigBright.Count; i++)
                    arrayM[nM + i] = new M(xFigBright[i], -1);
                nM += xFigBright.Count;

                //сортируем его для дальнейшей работы
                _ = new M(0, 0);
                for (var write = 0; write < arrayM.Length; write++)
                {
                    for (var sort = 0; sort < arrayM.Length - 1; sort++)
                    {
                        if (arrayM[sort].X > arrayM[sort + 1].X)
                        {
                            var buuf = new M(arrayM[sort + 1].X, arrayM[sort + 1].Dq);
                            arrayM[sort + 1] = arrayM[sort];
                            arrayM[sort] = buuf;
                        }
                    }
                }

                var Q = 0;
                List<int> xrl = new List<int>();
                List<int> xrr = new List<int>();
                // Особый случай для правой границы сегмента
                if (arrayM[0].X >= 0 && arrayM[0].Dq < 0)
                {
                    xrl.Add(0);
                    Q = -arrayM[1].Dq;
                }

                for (var i = 0; i < nM; i++)
                {
                    var x = arrayM[i].X;
                    var Qnew = Q + arrayM[i].Dq;
                    if (!(SetQ[0] <= Q && Q <= SetQ[1]) && (SetQ[0] <= Qnew && Qnew <= SetQ[1]))
                        xrl.Add((int)x);
                    else if ((SetQ[0] <= Q && Q <= SetQ[1]) && !(SetQ[0] <= Qnew && Qnew <= SetQ[1]))
                        xrr.Add((int)x);

                    Q = Qnew;
                }

                // Если не найдена правая граница последнего сегмента
                if (SetQ[0] <= Q && Q <= SetQ[1])
                    xrr.Add(pictureBox1.Height);

                //отрисовка результата ТМО
                for (var i = 0; i < xrr.Count; i++)
                {
                    g.DrawLine(DrawPen, new Point(xrr[i], Y), new Point(xrl[i], Y));
                }
            }
            tmo = true;
            pictureBox1.Image = picture;
        }
        #endregion

        #region Геометрические преобразования
        //Присвоение кодов для геометрических преобразований
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //коды для операций
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    {
                        //масштабирование
                        Geometric = 1;
                        OperationOnClick = 9;


                        break;
                    }
                case 1:
                    {
                        //поворот на произвольный относительно заданного центра
                        Geometric = 2;
                        OperationOnClick = 6;
                        break;
                    }
                case 2:
                    {
                        //поворот на 45 относительно заданного центра
                        FortyFiveRotate = true;
                        Geometric = 3;
                        OperationOnClick = 7;
                        break;
                    }
            }
        }

        //1 - Перемещение
        #region Перемещение
        //Кнопка для выбора операции перемещения
        private void button8_Click_1(object sender, EventArgs e)
        {
            OperationOnClick = 5;
        }


        private FigureInter selectedFigure;

        //выбор фигуры
        private void ThisFugure(MouseEventArgs e)
        {
            FigureIsClicked = false; // Сначала считаем, что фигура не выбрана

            for (int i = listFigure.Count - 1; i >= 0; i--)
            {
                if (listFigure[i].ThisFigure(e.X, e.Y))
                {
                    g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 10, 10);
                    FigureIsClicked = true;
                    selectedFigure = listFigure[i];
                    break;
                }
            }
        }

        //перемещение
        private void MoveFigure(MouseEventArgs e)
        {
            if (FigureIsClicked && selectedFigure != null)
            {
                selectedFigure.Move(e.X - pictureBoxMousePosition.X, e.Y - pictureBoxMousePosition.Y);
                g.Clear(pictureBox1.BackColor);

                if (Bezie == false)
                {
                    selectedFigure.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                }
                else
                {
                    selectedFigure.PaintingBezie(DrawPen);
                }

                pictureBox1.Image = picture;
                pictureBoxMousePosition = e.Location;
            }
        }

        //обработчик действия - движение мыши по области рисования
        private void PictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (OperationOnClick == 5) && (FigureIsClicked))
                MoveFigure(e);
        }
        #endregion

        //2 - Масштабирование по ОХ относительно центра фигуры
        #region Масштабирование
        private void ChangeSizeX(object sender, MouseEventArgs e)
        {
            if ((Geometric == 1) && (OperationOnClick == 9))
            {
                listFigure[listFigure.Count - 1].ChangeSizeX(pictureBox1.Height, new float[] { e.Delta, e.Delta });
                g.Clear(pictureBox1.BackColor);
                if (Bezie == false)
                {
                    listFigure[listFigure.Count - 1].FillIn(DrawPen, pictureBox1.Height, haveBorder);
                }
                else
                {
                    listFigure[listFigure.Count - 1].PaintingBezie(DrawPen);
                }
                pictureBox1.Image = picture;
            }
        }
        #endregion

        //3 - Произвольный поворот вокруг заданного центра
        #region Поворот на произвольный угол
        private void RotatePrandDeg(object sender, MouseEventArgs e)
        {
            var figurka1 = listFigure[listFigure.Count - 1];

            if ((Geometric == 2) && (OperationOnClick == 6))
            {
                figurka1.Rotation(e.Delta, pictureBox1.Height, textBox1, e, Geometric, rotationCenter);
                g.Clear(pictureBox1.BackColor);
                if (Bezie == false)
                {
                    figurka1.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                }
                else
                {
                    figurka1.PaintingBezie(DrawPen);
                }
                pictureBox1.Image = picture;
            }
        }
        #endregion


    
        //4 - Поворот на 45 градусов вокруг заданного центра
        #region Поворот на 45 градусов
        private void CertainRotate(object sender, MouseEventArgs e)
        {
            var figurka1 = listFigure[listFigure.Count - 1];

            if ((FortyFiveRotate) && (OperationOnClick == 7))
            {
                figurka1.RotateOnFortyFive(pictureBox1.Height, e, Geometric);
                g.Clear(pictureBox1.BackColor);
                if (Bezie == false)
                {
                    figurka1.FillIn(DrawPen, pictureBox1.Height, haveBorder);
                }
                else
                {
                    figurka1.PaintingBezie(DrawPen);
                }
                pictureBox1.Image = picture;
            }
        }
        #endregion

        #endregion

        //------

        #region Обработка палитры
        // Обработчик события выбора цвета
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox4.SelectedIndex) // выбор цвета
            {
                case 0:
                    DrawPen.Color = Color.Black;
                    break;
                case 1:
                    DrawPen.Color = Color.Red;
                    break;
                case 2:
                    DrawPen.Color = Color.Green;
                    break;
                case 3:
                    DrawPen.Color = Color.Blue;
                    break;
            }

        }
        #endregion


        //очистка формы
        private void button10_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = picture;
            g.Clear(Color.White);
            fig.GetPoints().Clear();
            listFigure.Clear();
            OperationOnClick = 0;
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void тМОToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
