using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Genetic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    cellColors[i, j] = SystemColors.Control;
                }
            }

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    cellDataSet[i, j] = 0;
                }
            }

            using (StreamReader fstream = new StreamReader("mazesList.txt", Encoding.GetEncoding(1251)))
            {
                while (true)
                {
                    string temp = fstream.ReadLine();
                    if (temp == null) break;

                    if (temp.Substring(0, 3) == "lab")
                    {
                        string[] subs = temp.Split(' ');
                        mazes.Add(subs[1]);
                    }
                }
            }

            string[] mazesString = new string[mazes.Count];
            for (int i = 0; i < mazes.Count; i++)
            {
                mazesString[i] = mazes[i];
            }
            comboBox1.Items.AddRange(mazesString);
        }

        Color[,] cellColors = new Color[25, 25];
        int[,] cellDataSet = new int[25, 25];
        bool programStart = true;
        List<string> mazes = new List<string>() { };
        bool paint;
        int[] startPoint = new int[2];
        int[] endPoint = new int[2];
        bool finished;
        bool startPointTag = true;
        int currentCursorPointX = 0;
        int currentCursorPointY = 0;
        bool thremauxSolving = false;
        //Color previousCursorColor = Color.White;
        //int previousCursorPointX = 0;
        //int previousCursorPointY = 0;
        List<double> mutationValues = new List<double>();
        int population = 0;
        int generations = 0;
        

        public bool isSpace(string phrase)
        {
            for (int i = 0; i < phrase.Length; i++)
            {
                if (phrase[i] == ' ')
                {
                    return true;
                }
            }

            return false;
        }

        public int[,] copyMatrix(int[,] matrix)
        {
            int[,] copyOfMatrix = new int[25, 25];

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    copyOfMatrix[i, j] = matrix[i, j];
                }
            }

            return copyOfMatrix;
        }

        public int[] returnColor(int colorNum)
        {
            int[] rgb = new int[3];

            if (colorNum > 0 && colorNum <= 204)
            {
                rgb[0] = 237;
                rgb[1] = 33 + colorNum;
                rgb[2] = 29;
                return rgb;
            }
            else if (colorNum > 204 && colorNum <= 412)
            {
                rgb[0] = 236 - (colorNum - 204);
                rgb[1] = 237;
                rgb[2] = 29;
                return rgb;
            }
            else if (colorNum > 412 && colorNum <= 620)
            {
                rgb[0] = 29;
                rgb[1] = 237;
                rgb[2] = 28 + (colorNum - 412);
                return rgb;
            }
            else if (colorNum > 620 && colorNum <= 816)
            {
                rgb[0] = 29;
                rgb[1] = 237 - (colorNum - 620);
                rgb[2] = 237;
                return rgb;
            }
            else
            {
                return rgb;
            }
        }

        public int[] returnPropColor(int colorCount, int colorNum)
        {
            int step = 816 / colorCount;
            return (returnColor(colorNum * step));
        }

        public void prepareMatrixForVaweAlg(int[,] matrix)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrix[i, j] == 1)
                    {
                        matrix[i, j] = -1;
                    }
                    if (matrix[i, j] == 6)
                    {
                        matrix[i, j] = 0;
                    }
                    if (matrix[i, j] == 7)
                    {
                        matrix[i, j] = 0;
                    }
                }
            }
        }

        public bool isCross(int x, int y, int[,] matrix)
        {
            int corners = 0;

            if (x < 24)
            {
                if (matrix[x + 1, y] != 1)
                {
                    corners++;
                }
            }
            if (x > 0)
            {
                if (matrix[x - 1, y] != 1)
                {
                    corners++;
                }
            }
            if (y < 24)
            {
                if (matrix[x, y + 1] != 1)
                {
                    corners++;
                }
            }
            if (y > 0)
            {
                if (matrix[x, y - 1] != 1)
                {
                    corners++;
                }
            }

            if (corners < 3)
            {
                return false;
            }
            else
            {
                generations++;
                population += corners;
                return true;
            }
        }

        public void wasHereOnCross(int x, int y, int[,] matrix)
        {
            if (matrix[x, y] == 2 || matrix[x, y] == 0)
            {
                matrix[x, y] = 4;
            }
            else if (matrix[x, y] == 3)
            {
                matrix[x, y] = 5;
            }
            else if (matrix[x, y] == 4)
            {
                matrix[x, y] = 5;
            }
        }

        public string getDirection(int directionNumber)
        {
            if (directionNumber == 1)
            {
                return "вверх";
            }
            else if (directionNumber == 2)
            {
                return "вниз";
            }
            else if (directionNumber == 3)
            {
                return "влево";
            }
            else if (directionNumber == 4)
            {
                return "вправо";
            }
            else
            {
                return "";
            }
        }

        public int stepUp(int x, int y, int[,] matrix)
        {
            return matrix[x - 1, y];
        }

        public int stepDown(int x, int y, int[,] matrix)
        {
            return matrix[x + 1, y];
        }

        public int stepLeft(int x, int y, int[,] matrix)
        {
            return matrix[x, y - 1];
        }

        public int stepRight(int x, int y, int[,] matrix)
        {
            return matrix[x, y + 1];
        }

        public int[,] createActions(int x, int y, int[,] matrix)
        {
            int[,] actions = new int[4, 4];

            actions[0, 0] = stepUp(x, y, matrix);
            actions[0, 1] = stepLeft(x, y, matrix);
            actions[0, 2] = stepRight(x, y, matrix);
            actions[0, 3] = stepDown(x, y, matrix);

            actions[1, 0] = stepDown(x, y, matrix);
            actions[1, 1] = stepRight(x, y, matrix);
            actions[1, 2] = stepLeft(x, y, matrix);
            actions[1, 3] = stepUp(x, y, matrix);

            actions[2, 0] = stepLeft(x, y, matrix);
            actions[2, 1] = stepDown(x, y, matrix);
            actions[2, 2] = stepUp(x, y, matrix);
            actions[2, 3] = stepRight(x, y, matrix);

            actions[3, 0] = stepRight(x, y, matrix);
            actions[3, 1] = stepUp(x, y, matrix);
            actions[3, 2] = stepDown(x, y, matrix);
            actions[3, 3] = stepLeft(x, y, matrix);

            return actions;
        }

        public int[,] createDirections()
        {
            int[,] directions = new int[4, 4];

            directions[0, 0] = 1;
            directions[0, 1] = 3;
            directions[0, 2] = 4;
            directions[0, 3] = 2;

            directions[1, 0] = 2;
            directions[1, 1] = 4;
            directions[1, 2] = 3;
            directions[1, 3] = 1;

            directions[2, 0] = 3;
            directions[2, 1] = 2;
            directions[2, 2] = 1;
            directions[2, 3] = 4;

            directions[3, 0] = 4;
            directions[3, 1] = 1;
            directions[3, 2] = 2;
            directions[3, 3] = 3;

            return directions;
        }

        public int[,,] createCoordinates(int x, int y)
        {
            int[,,] coordinates = new int[4, 4, 2];

            coordinates[0, 0, 0] = x - 1;
            coordinates[0, 0, 1] = y;
            coordinates[0, 1, 0] = x;
            coordinates[0, 1, 1] = y - 1;
            coordinates[0, 2, 0] = x;
            coordinates[0, 2, 1] = y + 1;
            coordinates[0, 3, 0] = x + 1;
            coordinates[0, 3, 1] = y;

            coordinates[1, 0, 0] = x + 1;
            coordinates[1, 0, 1] = y;
            coordinates[1, 1, 0] = x;
            coordinates[1, 1, 1] = y + 1;
            coordinates[1, 2, 0] = x;
            coordinates[1, 2, 1] = y - 1;
            coordinates[1, 3, 0] = x - 1;
            coordinates[1, 3, 1] = y;

            coordinates[2, 0, 0] = x;
            coordinates[2, 0, 1] = y - 1;
            coordinates[2, 1, 0] = x + 1;
            coordinates[2, 1, 1] = y;
            coordinates[2, 2, 0] = x - 1;
            coordinates[2, 2, 1] = y;
            coordinates[2, 3, 0] = x;
            coordinates[2, 3, 1] = y + 1;

            coordinates[3, 0, 0] = x;
            coordinates[3, 0, 1] = y + 1;
            coordinates[3, 1, 0] = x - 1;
            coordinates[3, 1, 1] = y;
            coordinates[3, 2, 0] = x + 1;
            coordinates[3, 2, 1] = y;
            coordinates[3, 3, 0] = x;
            coordinates[3, 3, 1] = y - 1;

            return coordinates;
        }

        public int[] goOnCross(int x, int y, string direction, int[,] matrix)
        {
            int[,] actions = createActions(x, y, matrix);
            int[,] directions = createDirections();
            int[,,] coordinates = createCoordinates(x, y);

            int[] action;
            int[] dir;
            int[,] coords;

            if (direction == "вверх")
            {
                action = new int[4] { actions[0, 0], actions[0, 1], actions[0, 2], actions[0, 3] };
                dir = new int[4] { directions[0, 0], directions[0, 1], directions[0, 2], directions[0, 3] };
                coords = new int[4, 2] { { coordinates[0, 0, 0], coordinates[0, 0, 1] }, { coordinates[0, 1, 0], coordinates[0, 1, 1] }, { coordinates[0, 2, 0], coordinates[0, 2, 1] }, { coordinates[0, 3, 0], coordinates[0, 3, 1] } };
            }
            else if (direction == "вниз")
            {
                action = new int[4] { actions[1, 0], actions[1, 1], actions[1, 2], actions[1, 3] };
                dir = new int[4] { directions[1, 0], directions[1, 1], directions[1, 2], directions[1, 3] };
                coords = new int[4, 2] { { coordinates[1, 0, 0], coordinates[1, 0, 1] }, { coordinates[1, 1, 0], coordinates[1, 1, 1] }, { coordinates[1, 2, 0], coordinates[1, 2, 1] }, { coordinates[1, 3, 0], coordinates[1, 3, 1] } };
            }
            else if (direction == "влево")
            {
                action = new int[4] { actions[2, 0], actions[2, 1], actions[2, 2], actions[2, 3] };
                dir = new int[4] { directions[2, 0], directions[2, 1], directions[2, 2], directions[2, 3] };
                coords = new int[4, 2] { { coordinates[2, 0, 0], coordinates[2, 0, 1] }, { coordinates[2, 1, 0], coordinates[2, 1, 1] }, { coordinates[2, 2, 0], coordinates[2, 2, 1] }, { coordinates[2, 3, 0], coordinates[2, 3, 1] } };
            }
            else
            {
                action = new int[4] { actions[3, 0], actions[3, 1], actions[3, 2], actions[3, 3] };
                dir = new int[4] { directions[3, 0], directions[3, 1], directions[3, 2], directions[3, 3] };
                coords = new int[4, 2] { { coordinates[3, 0, 0], coordinates[3, 0, 1] }, { coordinates[3, 1, 0], coordinates[3, 1, 1] }, { coordinates[3, 2, 0], coordinates[3, 2, 1] }, { coordinates[3, 3, 0], coordinates[3, 3, 1] } };
            }

            if (action[3] == 5 || action[3] == 1 || action[3] == 6)
            {
                if (action[1] != 1 && action[1] != 5 && action[1] != 6)
                {
                    if (action[1] == 4)
                    {
                        if (action[2] == 4)
                        {
                            if (action[0] == 4 || action[0] == 5 || action[0] == 6)
                            {
                                int[] answer = new int[3] { coords[2, 0], coords[2, 1], dir[2] };
                                return answer;
                            }
                            else
                            {
                                int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[2] };
                                return answer;
                            }
                        }
                        else if (action[2] == 0 || action[2] == 7)
                        {
                            int[] answer = new int[3] { coords[2, 0], coords[2, 1], dir[2] };
                            return answer;
                        }
                        else if (action[2] == 5 || action[2] == 1 || action[2] == 6)
                        {
                            if (action[0] == 5 || action[0] == 1 || action[0] == 6)
                            {
                                int[] answer = new int[3] { coords[1, 0], coords[1, 1], dir[1] };
                                return answer;
                            }
                            else
                            {
                                int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                                return answer;
                            }
                        }
                    }
                    else if (action[1] == 0)
                    {
                        int[] answer = new int[3] { coords[1, 0], coords[1, 1], dir[1] };
                        return answer;
                    }
                }
                else if (action[1] == 5 || action[1] == 1 || action[1] == 6)
                {
                    if (action[2] == 5 || action[2] == 1 || action[2] == 6)
                    {
                        if (action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            Console.WriteLine(x.ToString(), " ", y.ToString());
                            Console.WriteLine("Ошибка!");
                            MessageBox.Show("Ошибка во время работы алгоритма!");
                            int[] answer = new int[3] { 0, 0, 0 };
                            return answer;
                        }
                        else
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                    }
                    else if (action[2] == 4)
                    {
                        if (action[0] == 4 || action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            int[] answer = new int[3] { coords[2, 0], coords[2, 1], dir[2] };
                            return answer;
                        }
                        else if (action[0] == 0)
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                    }
                    else
                    {
                        int[] answer = new int[3] { coords[2, 0], coords[2, 1], dir[2] };
                        return answer;
                    }
                }
            }
            if (action[3] == 4)
            {
                if (action[2] == 4)
                {
                    if (action[1] == 4)
                    {
                        if (action[0] == 0)
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                        else if (action[0] == 4 || action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            int[] answer = new int[3] { coords[3, 0], coords[3, 1], dir[3] };
                            return answer;
                        }
                    }
                    else if (action[1] == 5 || action[1] == 1 || action[1] == 6)
                    {
                        if (action[0] == 0)
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                        else if (action[0] == 4 || action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            int[] answer = new int[3] { coords[3, 0], coords[3, 1], dir[3] };
                            return answer;
                        }
                    }
                    else
                    {
                        int[] answer = new int[3] { coords[1, 0], coords[1, 1], dir[1] };
                        return answer;
                    }
                }
                else if (action[2] == 5 || action[2] == 1 || action[2] == 6)
                {
                    if (action[1] == 4)
                    {
                        if (action[0] == 4 || action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            int[] answer = new int[3] { coords[3, 0], coords[3, 1], dir[3] };
                            return answer;
                        }
                        else
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                    }
                    else if (action[1] == 5 || action[1] == 1 || action[1] == 6)
                    {
                        if (action[0] == 5 || action[0] == 1 || action[0] == 6)
                        {
                            Console.WriteLine(x.ToString(), " ", y.ToString());
                            Console.WriteLine("Ошибка!");
                            MessageBox.Show("Ошибка во время работы алгоритма!");
                            int[] answer = new int[3] { 0, 0, 0 };
                            return answer;
                        }
                        else
                        {
                            int[] answer = new int[3] { coords[0, 0], coords[0, 1], dir[0] };
                            return answer;
                        }
                    }
                    else if (action[1] == 0)
                    {
                        int[] answer = new int[3] { coords[1, 0], coords[1, 1], dir[1] };
                        return answer;
                    }
                }
                else
                {
                    int[] answer = new int[3] { coords[2, 0], coords[2, 1], dir[2] };
                    return answer;
                }
            }

            Console.WriteLine(x.ToString(), " ", y.ToString());
            Console.WriteLine("Ошибка!");
            MessageBox.Show("Ошибка во время работы алгоритма!");
            int[] answerError = new int[3] { 0, 0, 0 };
            return answerError;
        }

        public bool isStepable(int x, int y, int num, string direction, int[,] matrix)
        {
            if (direction == "вверх")
            {
                if (x > 0)
                {
                    if (matrix[x - 1, y] == num)
                    {
                        return false;
                    }
                }
            }
            if (direction == "вниз")
            {
                if (x < 24)
                {
                    if (matrix[x + 1, y] == num)
                    {
                        return false;
                    }
                }
            }
            if (direction == "влево")
            {
                if (y > 0)
                {
                    if (matrix[x, y - 1] == num)
                    {
                        return false;
                    }
                }
            }
            if (direction == "вправо")
            {
                if (y < 24)
                {
                    if (matrix[x, y + 1] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int[] stepForward(int x, int y, string direction, int[,] matrix)
        {
            if (direction == "вверх")
            {
                if (x > 0)
                {
                    int[] result = new int[2] { x - 1, y };
                    return result;
                }
                else
                {
                    int[] result = new int[2] { x, y };
                    return result;
                }
            }
            else if (direction == "вниз")
            {
                if (x < 24)
                {
                    int[] result = new int[2] { x + 1, y };
                    return result;
                }
                else
                {
                    int[] result = new int[2] { x, y };
                    return result;
                }
            }
            else if (direction == "влево")
            {
                if (y > 0)
                {
                    int[] result = new int[2] { x, y - 1 };
                    return result;
                }
                else
                {
                    int[] result = new int[2] { x, y };
                    return result;
                }
            }
            else if (direction == "вправо")
            {
                if (y < 24)
                {
                    int[] result = new int[2] { x, y + 1 };
                    return result;
                }
                else
                {
                    int[] result = new int[2] { x, y };
                    return result;
                }
            }

            int[] resultError = new int[2] { x, y };
            return resultError;
        }

        public void wasHere(int x, int y, int[,] matrix)
        {
            if (matrix[x, y] == 0)
            {
                matrix[x, y] = 2;
            }
            else if (matrix[x, y] == 2)
            {
                matrix[x, y] = 3;
            }
        }

        public string changeDirection(int x, int y, int num, string direction, int[,] matrix)
        {
            if (direction == "вверх")
            {
                if (y > 0)
                {
                    if (matrix[x, y - 1] == num)
                    {
                        return "влево";
                    }
                }
                if (y < 24)
                {
                    if (matrix[x, y + 1] == num)
                    {
                        return "вправо";
                    }
                }
                if (x < 24)
                {
                    if (matrix[x + 1, y] == num)
                    {
                        return "вниз";
                    }
                }
            }
            if (direction == "вниз")
            {
                if (y > 0)
                {
                    if (matrix[x, y - 1] == num)
                    {
                        return "влево";
                    }
                }
                if (y < 24)
                {
                    if (matrix[x, y + 1] == num)
                    {
                        return "вправо";
                    }
                }
                if (x > 0)
                {
                    if (matrix[x - 1, y] == num)
                    {
                        return "вверх";
                    }
                }
            }
            if (direction == "влево")
            {
                if (x > 0)
                {
                    if (matrix[x - 1, y] == num)
                    {
                        return "вверх";
                    }
                }
                if (x < 24)
                {
                    if (matrix[x + 1, y] == num)
                    {
                        return "вниз";
                    }
                }
                if (y < 24)
                {
                    if (matrix[x, y + 1] == num)
                    {
                        return "вправо";
                    }
                }
            }
            if (direction == "вправо")
            {
                if (x > 0)
                {
                    if (matrix[x - 1, y] == num)
                    {
                        return "вверх";
                    }
                }
                if (x < 24)
                {
                    if (matrix[x + 1, y] == num)
                    {
                        return "вниз";
                    }
                }
                if (y > 0)
                {
                    if (matrix[x, y - 1] == num)
                    {
                        return "влево";
                    }
                }
            }

            return "нет";
        }

        // todo место для написания следующего метода

        public void paintTable()
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 0)
                    {
                        cellColors[j, i] = SystemColors.Control;
                    }
                    else if (cellDataSet[j, i] == 1)
                    {
                        cellColors[j, i] = Color.Black;
                    }
                    else if (cellDataSet[j, i] == 6)
                    {
                        cellColors[j, i] = Color.Blue;
                        startPoint[0] = j;
                        startPoint[1] = i;
                        Console.WriteLine("Начальная точка: " + startPoint[0].ToString() + " " + startPoint[1].ToString());
                    }
                    else if (cellDataSet[j, i] == 7)
                    {
                        cellColors[j, i] = Color.Red;
                        endPoint[0] = j;
                        endPoint[1] = i;
                        Console.WriteLine("Конечная точка: " + endPoint[0].ToString() + " " + endPoint[1].ToString());
                    }
                }
            }

            Thread.Sleep(100);
            pictureBox1.Refresh();
        }

        public void refreshTablePaint()
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 0)
                    {
                        cellColors[j, i] = SystemColors.Control;
                    }
                    else if (cellDataSet[j, i] == 1)
                    {
                        cellColors[j, i] = Color.Black;
                    }
                    else if (cellDataSet[j, i] == 2 || cellDataSet[j, i] == 3)
                    {
                        cellColors[j, i] = Color.Yellow;
                    }
                    else if (cellDataSet[j, i] == 4)
                    {
                        cellColors[j, i] = Color.DarkRed;
                    }
                    else if (cellDataSet[j, i] == 6)
                    {
                        cellColors[j, i] = Color.Blue;
                    }
                    else if (cellDataSet[j, i] == 7)
                    {
                        cellColors[j, i] = Color.Red;
                    }
                }
            }

            Thread.Sleep(100);
            pictureBox1.Refresh();
        }

        public void refreshVaweTablePaint(int[,] matrix, int waveAlgVal)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrix[j, i] == -1)
                    {
                        cellColors[j, i] = Color.Black;
                    }
                    else if (matrix[j, i] == 0)
                    {
                        cellColors[j, i] = SystemColors.Control;
                    }
                    else if (matrix[j, i] > 0 && matrix[j, i] < 1000) {
                        cellColors[j, i] = Color.FromArgb(255, returnPropColor(waveAlgVal, matrix[j, i])[0],
                            returnPropColor(waveAlgVal, matrix[j, i])[1],
                            returnPropColor(waveAlgVal, matrix[j, i])[2]);
                    }
                    else if (matrix[j, i] == 1000)
                    {
                        cellColors[j, i] = Color.White;
                    }
                }
            }

            Thread.Sleep(100);
            pictureBox1.Refresh();
        }

        public void refreshTremauxTablePaint(int[,] matrix)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrix[j, i] == 1)
                    {
                        cellColors[j, i] = Color.Black;
                    }
                    else if (matrix[j, i] == 6)
                    {
                        cellColors[j, i] = Color.Blue;
                    }
                    else if (matrix[j, i] == 7)
                    {
                        cellColors[j, i] = Color.Red;
                    }
                    else if (matrix[j, i] == 4)
                    {
                        cellColors[j, i] = Color.LightBlue;
                    }
                    else if (matrix[j, i] == 5)
                    {
                        cellColors[j, i] = Color.Purple;
                    }
                    else if (matrix[j, i] == 9)
                    {
                        cellColors[j, i] = Color.Yellow;
                    }
                    else
                    {
                        cellColors[j, i] = Color.White;
                    }

                    if (j == currentCursorPointX && i == currentCursorPointY)
                    {
                        if (thremauxSolving)
                        {
                            cellColors[j, i] = Color.Green;
                        }
                        // todo делаю тут отрисовку курсора
                    }
                }
            }

            Thread.Sleep(100);
            pictureBox1.Refresh();
        }

        //public void startToFindTheWay(int[] currentPoint, int[] endPoint)
        //{
        //    int x = currentPoint[0];
        //    int y = currentPoint[1];

        //    int xe = endPoint[0];
        //    int ye = endPoint[1];

        //    cellDataSet[x, y] = 2;
        //    // фиксируем наше нахождение в конечной точке
        //    if (xe == x && ye == y)
        //    {
        //        if (paint)
        //        {
        //            refreshTablePaint();
        //        }
        //        paint = false;
        //        finished = true;
        //        Console.WriteLine("Всё!");
        //        return;
        //    }

        //    // фиксируем наличие перекрёстка
        //    if (isCross(x, y, cellDataSet))
        //    {
        //        Console.WriteLine("Перекрёсток!");
        //        return;
        //    }

        //    try
        //    {
        //        if (cellDataSet[x + 1, y] == 0 || cellDataSet[x + 1, y] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x + 1, y }, endPoint);
        //        }
        //    }
        //    catch (IndexOutOfRangeException e)
        //    {
        //        try
        //        {
        //            if (cellDataSet[x - 1, y] == 0 || cellDataSet[x - 1, y] == 7)
        //            {
        //                if (paint)
        //                {
        //                    refreshTablePaint();
        //                }
        //                findTheWay(new int[2] { x - 1, y }, endPoint);
        //            }
        //        }
        //        catch { }
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            if (cellDataSet[x, y - 1] == 0 || cellDataSet[x, y - 1] == 7)
        //            {
        //                if (paint)
        //                {
        //                    refreshTablePaint();
        //                }
        //                findTheWay(new int[2] { x, y - 1 }, endPoint);
        //            }
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                if (cellDataSet[x, y + 1] == 0 || cellDataSet[x, y + 1] == 7)
        //                {
        //                    if (paint)
        //                    {
        //                        refreshTablePaint();
        //                    }
        //                    findTheWay(new int[2] { x, y + 1 }, endPoint);
        //                }
        //            }
        //            catch { }
        //        }
        //    }
        //}

        //public void findTheWay(int[] currentPoint, int[] endPoint)
        //{
        //    for (int i = 0; i < 25; i++)
        //    {
        //        for (int j = 0; j < 25; j++)
        //        {
        //            Console.Write(cellDataSet[j, i].ToString() + " ");
        //        }
        //        Console.WriteLine();
        //    }

        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine();

        //    int x = currentPoint[0];
        //    int y = currentPoint[1];

        //    int xe = endPoint[0];
        //    int ye = endPoint[1];

        //    if (isCross(x, y, cellDataSet))
        //    {
        //        Console.WriteLine("Перекрёсток!");
        //    }

        //    cellDataSet[x, y] = 2;
        //    if (xe == x && ye == y)
        //    {
        //        if (paint)
        //        {
        //            refreshTablePaint();
        //        }
        //        paint = false;
        //        finished = true;
        //        Console.WriteLine("Всё!");
        //        return;
        //    }

        //    if (y > 0 && y < 24)
        //    {
        //        if (cellDataSet[x, y-1] == 0 || cellDataSet[x, y-1] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x, y - 1 }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //        if (cellDataSet[x, y+1] == 0 || cellDataSet[x, y+1] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x, y + 1 }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (x > 0 && x < 24)
        //    {
        //        if (cellDataSet[x+1, y] == 0 || cellDataSet[x+1, y] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x + 1, y }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //        if (cellDataSet[x-1, y] == 0 || cellDataSet[x-1, y] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x - 1, y }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (x == 0)
        //    {
        //        if (cellDataSet[x+1, y] == 0 || cellDataSet[x+1, y] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x + 1, y }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (x == 24)
        //    {
        //        if (cellDataSet[x-1, y] == 0 || cellDataSet[x-1, y] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x-1, y}, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (y == 0)
        //    {
        //        if (cellDataSet[x, y + 1] == 0 || cellDataSet[x, y+1] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x, y + 1 }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (y == 24)
        //    {
        //        if (cellDataSet[x, y-1] == 0 || cellDataSet[x, y-1] == 7)
        //        {
        //            if (paint)
        //            {
        //                refreshTablePaint();
        //            }
        //            findTheWay(new int[2] { x, y - 1 }, endPoint);
        //            if (finished)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (!finished)
        //    {
        //        cellDataSet[x, y] = 3;
        //        if (paint)
        //        {
        //            refreshTablePaint();
        //        }
        //    }
        //}











        public bool isLock(int x, int y, int passNumber)
        {
            int lockedSides = 0;

            try
            {
                if (cellDataSet[x + 1, y] != passNumber)
                {
                    lockedSides++;
                }
                if (cellDataSet[x + 1, y] == 7)
                {
                    return false;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x - 1, y] != passNumber)
                {
                    lockedSides++;
                }
                if (cellDataSet[x - 1, y] == 7)
                {
                    return false;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y + 1] != passNumber)
                {
                    lockedSides++;
                }
                if (cellDataSet[x, y + 1] == 7)
                {
                    return false;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y - 1] != passNumber)
                {
                    lockedSides++;
                }
                if (cellDataSet[x, y - 1] == 7)
                {
                    return false;
                }
            }
            catch { }

            if (lockedSides > 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void startGeneticAlgOnCross(int[] start_point)
        {
            int x = start_point[0];
            int y = start_point[1];
            refreshTablePaint();

            List<int> xCoordinatesOfMoving = new List<int>() { };
            List<int> yCoordinatesOfMoving = new List<int>() { };

            try
            {
                if (cellDataSet[x + 1, y] == 0)
                {
                    xCoordinatesOfMoving.Add(x + 1);
                    yCoordinatesOfMoving.Add(y);
                }
                else if (cellDataSet[x + 1, y] == 7)
                {
                    return;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x - 1, y] == 0)
                {
                    xCoordinatesOfMoving.Add(x - 1);
                    yCoordinatesOfMoving.Add(y);
                }
                else if (cellDataSet[x - 1, y] == 7)
                {
                    return;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y + 1] == 0)
                {
                    xCoordinatesOfMoving.Add(x);
                    yCoordinatesOfMoving.Add(y + 1);
                }
                else if (cellDataSet[x, y + 1] == 7)
                {
                    return;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y - 1] == 0)
                {
                    xCoordinatesOfMoving.Add(x);
                    yCoordinatesOfMoving.Add(y - 1);
                }
                else if (cellDataSet[x, y - 1] == 7)
                {
                    return;
                }
            }
            catch { }

            double mutationProbability = 0.0;
            for (int i = 0; i < xCoordinatesOfMoving.Count; i++)
            {
                if (geneticAlgGoStraight(start_point, new int[2] {xCoordinatesOfMoving[i], yCoordinatesOfMoving[i] }))
                {
                    mutationProbability += 0.25;
                }
            }

            mutationValues.Add(mutationProbability);
        }

        public bool geneticAlgOnCross(int[] last_point, int[] current_point)
        {
            int x = current_point[0];
            int y = current_point[1];
            cellDataSet[x, y] = 3;
            refreshTablePaint();

            List<int> xCoordinatesOfMoving = new List<int>() { };
            List<int> yCoordinatesOfMoving = new List<int>() { };

            try
            {
                if ((cellDataSet[x + 1, y] == 0) && ((x + 1 != endPoint[0]) || (y != endPoint[1])) && (x + 1 != last_point[0] || y != last_point[1]))
                {
                    xCoordinatesOfMoving.Add(x + 1);
                    yCoordinatesOfMoving.Add(y);
                }
                else if ((cellDataSet[x + 1, y] == 7) && ((x + 1 == endPoint[0]) || (y == endPoint[1])))
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x - 1, y] == 0) && ((x - 1 != endPoint[0]) || (y != endPoint[1])) && (x - 1 != last_point[0] || y != last_point[1]))
                {
                    xCoordinatesOfMoving.Add(x - 1);
                    yCoordinatesOfMoving.Add(y);
                }
                else if ((cellDataSet[x - 1, y] == 7) && ((x - 1 == endPoint[0]) || (y == endPoint[1])))
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x, y - 1] == 0) && ((x != endPoint[0]) || (y - 1 != endPoint[1])) && (x != last_point[0] || y - 1 != last_point[1]))
                {
                    xCoordinatesOfMoving.Add(x);
                    yCoordinatesOfMoving.Add(y - 1);
                }
                else if ((cellDataSet[x, y - 1] == 7) && ((x == endPoint[0]) || (y - 1 == endPoint[1])))
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x, y + 1] == 0) && ((x != endPoint[0]) || (y + 1 != endPoint[1])) && (x != last_point[0] || y + 1 != last_point[1]))
                {
                    xCoordinatesOfMoving.Add(x);
                    yCoordinatesOfMoving.Add(y + 1);
                }
                else if ((cellDataSet[x, y + 1] == 7) && ((x == endPoint[0]) || (y + 1 == endPoint[1])))
                {
                    return true;
                }
            }
            catch { }

            if (xCoordinatesOfMoving.Count == 0)
            {
                return false;
            }

            double mutationProbability = 0.0;
            for (int i = 0; i < xCoordinatesOfMoving.Count; i++)
            {
                if (geneticAlgGoStraight(current_point, new int[2] { xCoordinatesOfMoving[i], yCoordinatesOfMoving[i] }))
                {
                    mutationProbability += 0.33;
                }
            }

            if (mutationProbability == 0.99)
            {
                mutationProbability = 1.0;
            }
            mutationValues.Add(mutationProbability);
            return true;
        }

        public bool geneticAlgGoStraight(int[] last_point, int[] current_point)
        {
            int x = current_point[0];
            int y = current_point[1];
            cellDataSet[x, y] = 2;
            refreshTablePaint();
            bool stepDone = false;

            if (isLock(x, y, 0))
            {
                geneticAlgHighlightLock(current_point);
                return false;
            }
            else
            {
                if (isCross(x, y, cellDataSet))
                {
                    bool result = geneticAlgOnCross(last_point, current_point);
                    return result;
                }
                else
                {
                    try
                    {
                        if ((cellDataSet[x + 1, y] == 0) && ((x + 1 != endPoint[0]) || (y != endPoint[1])))
                        {
                            stepDone = true;
                            bool result = geneticAlgGoStraight(new int[2] { x, y }, new int[2] { x + 1, y });
                            return result;
                        }
                        else if ((x + 1 == endPoint[0]) && (y == endPoint[1]))
                        {
                            return true;
                        }
                    }
                    catch { }

                    try
                    {
                        if (!stepDone && (cellDataSet[x - 1, y] == 0) && ((x - 1 != endPoint[0]) || (y != endPoint[1])))
                        {
                            stepDone = true;
                            bool result = geneticAlgGoStraight(new int[2] { x, y }, new int[2] { x - 1, y });
                            return result;
                        }
                        else if ((x - 1 == endPoint[0]) && (y == endPoint[1]))
                        {
                            return true;
                        }
                    }
                    catch { }

                    try
                    {
                        if (!stepDone && (cellDataSet[x, y + 1] == 0) && ((x != endPoint[0]) || (y + 1 != endPoint[1])))
                        {
                            stepDone = true;
                            bool result = geneticAlgGoStraight(new int[2] { x, y }, new int[2] { x, y + 1 });
                            return result;
                        }
                        else if ((x == endPoint[0]) && (y + 1 == endPoint[1]))
                        {
                            return true;
                        }
                    }
                    catch { }

                    try
                    {
                        if (!stepDone && (cellDataSet[x, y - 1] == 0) && ((x != endPoint[0]) || (y - 1 != endPoint[1])))
                        {
                            bool result = geneticAlgGoStraight(new int[2] { x, y }, new int[2] { x, y - 1 });
                            return result;
                        }
                        else if ((x == endPoint[0]) && (y - 1 == endPoint[1]))
                        {
                            return true;
                        }
                    }
                    catch { }

                    return false;
                }
            }
        }

        public void geneticAlgHighlightLock(int[] point)
        {
            int x = point[0];
            int y = point[1];

            cellDataSet[x, y] = 4;
            refreshTablePaint();
            bool stepDone = false;

            try
            {
                if (cellDataSet[x + 1, y] == 2)
                {
                    stepDone = true;
                    geneticAlgHighlightLock(new int[2] { x + 1, y });
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x - 1, y] == 2)
                {
                    stepDone = true;
                    geneticAlgHighlightLock(new int[2] { x - 1, y });
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x, y - 1] == 2)
                {
                    stepDone = true;
                    geneticAlgHighlightLock(new int[2] { x, y - 1 });
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x, y + 1] == 2)
                {
                    stepDone = true;
                    geneticAlgHighlightLock(new int[2] { x, y + 1 });
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x + 1, y] == 3)
                {
                    stepDone = true;
                    if (!geneticAlgLockCross(new int[2] { x + 1, y }))
                    {
                        if (!geneticAlgLockCrossIsItStart(new int[2] { x + 1, y }))
                        {
                            int[] crossPoint = geneticAlgLockCrossFindWay(new int[2] { x + 1, y });
                            geneticAlgHighlightLock(crossPoint);
                        }
                        else
                        {
                            return;
                        }
                    }
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x - 1, y] == 3)
                {
                    stepDone = true;
                    if (!geneticAlgLockCross(new int[2] { x - 1, y }))
                    {
                        if (!geneticAlgLockCrossIsItStart(new int[2] { x - 1, y }))
                        {
                            int[] crossPoint = geneticAlgLockCrossFindWay(new int[2] { x - 1, y });
                            geneticAlgHighlightLock(crossPoint);
                        }
                        else
                        {
                            return;
                        }
                    }
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x, y + 1] == 3)
                {
                    stepDone = true;
                    if (!geneticAlgLockCross(new int[2] { x, y + 1 }))
                    {
                        if (!geneticAlgLockCrossIsItStart(new int[2] { x, y + 1 }))
                        {
                            int[] crossPoint = geneticAlgLockCrossFindWay(new int[2] { x, y + 1 });
                            geneticAlgHighlightLock(crossPoint);
                        }
                        else
                        {
                            return;
                        }
                    }
                    return;
                }
            }
            catch { }

            try
            {
                if (!stepDone && cellDataSet[x, y - 1] == 3)
                {
                    stepDone = true;
                    if (!geneticAlgLockCross(new int[2] { x, y - 1 }))
                    {
                        if (!geneticAlgLockCrossIsItStart(new int[2] { x, y - 1 }))
                        {
                            int[] crossPoint = geneticAlgLockCrossFindWay(new int[2] { x, y - 1 });
                            geneticAlgHighlightLock(crossPoint);
                        }
                        else
                        {
                            return;
                        }
                    }
                    return;
                }
            }
            catch { }
        }

        // TODO поставить точку останова тут
        public bool geneticAlgLockCross(int[] point)
        {
            int x = point[0];
            int y = point[1];

            int enters = 0;

            try
            {
                if ((cellDataSet[x + 1, y] == 2) || (cellDataSet[x + 1, y] == 0) || (cellDataSet[x + 1, y] == 3))
                {
                    enters++;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x - 1, y] == 2) || (cellDataSet[x - 1, y] == 0) || (cellDataSet[x + 1, y] == 3))
                {
                    enters++;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x, y + 1] == 2) || (cellDataSet[x, y + 1] == 0) || (cellDataSet[x + 1, y] == 3))
                {
                    enters++;
                }
            }
            catch { }

            try
            {
                if ((cellDataSet[x, y - 1] == 2) || (cellDataSet[x, y - 1] == 0) || (cellDataSet[x + 1, y] == 3))
                {
                    enters++;
                }
            }
            catch { }

            if (enters > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool geneticAlgLockCrossIsItStart(int[] point)
        {
            int x = point[0];
            int y = point[1];

            try
            {
                if (cellDataSet[x + 1, y] == 6)
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x - 1, y] == 6)
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y + 1] == 6)
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y - 1] == 6)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public int[] geneticAlgLockCrossFindWay(int[] point)
        {
            int x = point[0];
            int y = point[1];

            int xWay = point[0];
            int yWay = point[1];

            cellDataSet[x, y] = 4;
            refreshTablePaint();

            try
            {
                if (cellDataSet[x + 1, y] == 2)
                {
                    xWay++;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x - 1, y] == 2)
                {
                    xWay--;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y + 1] == 2)
                {
                    yWay++;
                }
            }
            catch { }

            try
            {
                if (cellDataSet[x, y - 1] == 2)
                {
                    yWay--;
                }
            }
            catch { }

            return new int[2] { xWay, yWay };
        }














        public bool findTheWayWave(int[] endPoint, int lastPointVal, int[,] matrix)
        {
            List<int> currentPointsX = new List<int>();
            List<int> currentPointsY = new List<int>();

            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    if (matrix[x, y] == lastPointVal)
                    {
                        currentPointsX.Add(x);
                        currentPointsY.Add(y);
                    }
                }
            }

            int xe = endPoint[0];
            int ye = endPoint[1];

            for (int i = 0; i < currentPointsX.Count; i++)
            {
                int x = currentPointsX[i];
                int y = currentPointsY[i];

                if (xe == x && ye == y)
                {
                    Console.WriteLine("Всё!");
                    Console.WriteLine(currentPointsX[i].ToString() + " " + currentPointsY[i].ToString());
                    refreshVaweTablePaint(matrix, lastPointVal);
                    return true;
                }
                if (y > 0 && y < 24)
                {
                    if (matrix[x, y - 1] == 0 || matrix[x, y - 1] > lastPointVal + 1)
                    {
                        matrix[x, y - 1] = lastPointVal + 1;
                    }
                    if (matrix[x, y + 1] == 0 || matrix[x, y + 1] > lastPointVal + 1)
                    {
                        matrix[x, y + 1] = lastPointVal + 1;
                    }
                }
                if (x > 0 && x < 24)
                {
                    if (matrix[x + 1, y] == 0 || matrix[x + 1, y] > lastPointVal + 1)
                    {
                        matrix[x + 1, y] = lastPointVal + 1;
                    }
                    if (matrix[x - 1, y] == 0 || matrix[x - 1, y] > lastPointVal + 1)
                    {
                        matrix[x - 1, y] = lastPointVal + 1;
                    }
                }
                if (x == 0)
                {
                    if (matrix[x + 1, y] == 0 || matrix[x + 1, y] > lastPointVal + 1)
                    {
                        matrix[x + 1, y] = lastPointVal + 1;
                    }
                }
                if (x == 24)
                {
                    if (matrix[x - 1, y] == 0 || matrix[x - 1, y] > lastPointVal + 1)
                    {
                        matrix[x - 1, y] = lastPointVal + 1;
                    }
                }
                if (y == 0)
                {
                    if (matrix[x, y + 1] == 0 || matrix[x, y + 1] > lastPointVal + 1)
                    {
                        matrix[x, y + 1] = lastPointVal + 1;
                    }
                }
                if (y == 24)
                {
                    if (matrix[x, y - 1] == 0 || matrix[x, y - 1] > lastPointVal + 1)
                    {
                        matrix[x, y - 1] = lastPointVal + 1;
                    }
                }
            }
            refreshVaweTablePaint(matrix, lastPointVal);

            return false;
        }

        public void findTheWayTremaux(int[] startPoint, int[] endPoint, int[,] matrix)
        {
            int[] currentPoint = new int[2];
            currentPoint[0] = startPoint[0];
            currentPoint[1] = startPoint[1];
            string direction = "";
            int cycle = 0;
            //previousCursorPointX = startPoint[0];
            //previousCursorPointY = startPoint[1];
            //previousCursorColor = Color.Blue;
            thremauxSolving = true;

            if (currentPoint[0] == 0)
            {
                if (matrix[currentPoint[0] + 1, currentPoint[1]] == 0)
                {
                    direction = "вниз";
                }
            }
            else if (currentPoint[0] == 24)
            {
                if (matrix[currentPoint[0] - 1, currentPoint[0]] == 0)
                {
                    direction = "вверх";
                }
            }
            else if (currentPoint[1] == 0)
            {
                if (matrix[currentPoint[0], currentPoint[1] + 1] == 0)
                {
                    direction = "вправо";
                }
            }
            else if (currentPoint[1] == 24)
            {
                if (matrix[currentPoint[0], currentPoint[1] - 1] == 0)
                {
                    direction = "влево";
                }
            }

            if (direction == "")
            {
                if (matrix[currentPoint[0] + 1, currentPoint[1]] == 0)
                {
                    direction = "вниз";
                }
                else if (matrix[currentPoint[0] - 1, currentPoint[1]] == 0)
                {
                    direction = "вверх";
                }
                else if (matrix[currentPoint[0], currentPoint[1] + 1] == 0)
                {
                    direction = "вправо";
                }
                else if (matrix[currentPoint[0], currentPoint[1] - 1] == 0)
                {
                    direction = "влево";
                }
            }

            while (currentPoint[0] != endPoint[0] || currentPoint[1] != endPoint[1])
            {
                int x = currentPoint[0];
                int y = currentPoint[1];

                currentCursorPointX = currentPoint[0];
                currentCursorPointY = currentPoint[1];

                refreshTremauxTablePaint(matrix);

                if (isCross(x, y, matrix))
                {
                    matrix[x, y] = 8;
                    if (cycle > 1)
                    {
                        if (direction == "вверх")
                        {
                            wasHereOnCross(x + 1, y, matrix);
                        }
                        else if (direction == "вниз")
                        {
                            wasHereOnCross(x - 1, y, matrix);
                        }
                        else if (direction == "влево")
                        {
                            wasHereOnCross(x, y + 1, matrix);
                        }
                        else if (direction == "вправо")
                        {
                            wasHereOnCross(x, y - 1, matrix);
                        }
                    }

                    int[] result = goOnCross(x, y, direction, matrix);
                    currentPoint[0] = result[0];
                    currentPoint[1] = result[1];
                    direction = getDirection(result[2]);
                    wasHereOnCross(currentPoint[0], currentPoint[1], matrix);
                    cycle = 0;
                }
                else
                {
                    if (isStepable(x, y, 1, direction, matrix) &&
                        isStepable(x, y, 3, direction, matrix) &&
                        isStepable(x, y, 5, direction, matrix))
                    {
                        currentPoint[0] = stepForward(x, y, direction, matrix)[0];
                        currentPoint[1] = stepForward(x, y, direction, matrix)[1];
                        wasHere(x, y, matrix);
                        cycle++;
                    }
                    else
                    {
                        if (changeDirection(x, y, 0, direction, matrix) == "нет")
                        {
                            if (changeDirection(x, y, 2, direction, matrix) == "нет")
                            {
                                if (changeDirection(x, y, 4, direction, matrix) == "нет")
                                {
                                    if (changeDirection(x, y, 8, direction, matrix) == "нет")
                                    {
                                        MessageBox.Show("Нет пути!");
                                        Console.WriteLine(x.ToString(), " ", y.ToString());
                                        break;
                                    }
                                    else
                                    {
                                        direction = changeDirection(x, y, 8, direction, matrix);
                                    }
                                }
                                else
                                {
                                    direction = changeDirection(x, y, 4, direction, matrix);
                                }
                            }
                            else
                            {
                                direction = changeDirection(x, y, 2, direction, matrix);
                                wasHere(x, y, matrix);
                            }
                        }
                        else
                        {
                            direction = changeDirection(x, y, 0, direction, matrix);
                        }
                    }
                }

                refreshTremauxTablePaint(matrix);
            }
        }

        // перерисовка панели
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 501; i = i + 20)
            {
                e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(0, i), new Point(500, i));
                e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(i, 0), new Point(i, 500));
            }

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(cellColors[i, j]), new Rectangle(i * 20 + 1, j * 20 + 1, 19, 19));
                }
            }
        }

        // todo обработка клика мышки по панели
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X / 20;
            int y = e.Y / 20;

            if (e.Button.ToString() == "Left")
            {
                if (cellDataSet[x, y] == 0)
                {
                    cellDataSet[x, y] = 1;
                    cellColors[x, y] = Color.Black;
                }
                else
                {
                    cellDataSet[x, y] = 0;
                    cellColors[x, y] = Color.White;
                }
            }
            else
            {
                if (startPointTag)
                {
                    cellDataSet[x, y] = 6;
                    cellColors[x, y] = Color.Blue;
                    startPointTag = !startPointTag;
                }
                else
                {
                    cellDataSet[x, y] = 7;
                    cellColors[x, y] = Color.Red;
                    startPointTag = !startPointTag;
                }
            }

            pictureBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите имя лабиринта" || textBox1.Text == "" || isSpace(textBox1.Text))
            {
                MessageBox.Show("Введите имя лабиринта, который хотите сохранить без пробелов в названии!");
            }
            else
            {
                bool uniq = true;

                for (int i = 0; i < mazes.Count; i++)
                {
                    if (mazes[i] == textBox1.Text)
                    {
                        uniq = false;
                        break;
                    }
                }

                if (uniq)
                {
                    using (StreamWriter writer = new StreamWriter(new FileStream("mazesList.txt", FileMode.Append), Encoding.GetEncoding(1251)))
                    {
                        string text = "lab " + textBox1.Text;
                        writer.WriteLine(text);

                        for (int i = 0; i < 25; i++)
                        {
                            text = "";
                            for (int j = 0; j < 25; j++)
                            {
                                if (cellDataSet[j, i] != 0 && cellDataSet[j, i] != 1 && cellDataSet[j, i] != 6 && cellDataSet[j, i] != 7)
                                {
                                    text += "0";
                                }
                                else
                                {
                                    text += cellDataSet[j, i].ToString();
                                }
                            }

                            writer.WriteLine(text);
                        }
                    }
                    mazes.Add(textBox1.Text);
                    comboBox1.Items.Add(textBox1.Text);
                }
                else
                {
                    MessageBox.Show("Такое название уже имеется, введите другое!");
                }

                programStart = !programStart;
                textBox1.Text = "Введите имя лабиринта";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (programStart)
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
                programStart = !programStart;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (StreamReader reader = new StreamReader("mazesList.txt", Encoding.GetEncoding(1251)))
            {
                bool found = false;

                while (true)
                {
                    string text;
                    if ((text = reader.ReadLine()) != null)
                    {
                        if (text.Substring(0, 3) == "lab")
                        {
                            string[] subs = text.Split(' ');
                            if (subs[1] == comboBox1.Text)
                            {
                                found = !found;

                                for (int i = 0; i < 25; i++)
                                {
                                    string str = reader.ReadLine();
                                    for (int j = 0; j < 25; j++)
                                    {
                                        string numStr = str[j].ToString();
                                        int num = Convert.ToInt32(numStr);
                                        cellDataSet[j, i] = num;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (!found)
                {
                    MessageBox.Show("Такой лабиринт не найден!");
                }
            }

            paintTable();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = " ";
            textBox3.Text = " ";
            textBox4.Text = " ";
            textBox5.Text = " ";

            groupBox3.Visible = true;
            double mutation = 0;
            mutationValues.Clear();
            population = 0;
            generations = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            paint = true;
            finished = false;
            // findTheWay(startPoint, endPoint);
            startGeneticAlgOnCross(startPoint);

            stopwatch.Stop();
            TimeSpan stopwatchEllapsed = stopwatch.Elapsed;
            int seconds = Convert.ToInt32(stopwatchEllapsed.TotalSeconds);
            int miliseconds = Convert.ToInt32(stopwatchEllapsed.TotalMilliseconds) % 100;
            textBox2.Text = seconds.ToString() + "," + miliseconds.ToString();

            int touchedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] != 1 && cellDataSet[j, i] != 0)
                    {
                        touchedCells += 1;
                    }
                }
            }
            textBox3.Text = touchedCells.ToString();

            int abourtedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 3)
                    {
                        abourtedCells += 1;
                    }
                }
            }
            textBox4.Text = abourtedCells.ToString();

            int wayLength = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 2)
                    {
                        wayLength += 1;
                    }
                }
            }
            textBox5.Text = wayLength.ToString();

            for (int i = 0; i < mutationValues.Count; i++)
            {
                mutation += mutationValues[i];

            }
            mutation /= mutationValues.Count;

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    Console.Write(cellDataSet[j, i]);
                }
                Console.WriteLine();
            }

            generationsBox.Text = generations.ToString();
            populationBox.Text = population.ToString();
            mutationTypeBox.Text = "Мутация для вещественных особей";
            mutationProbabilityBox.Text = Math.Round(mutation, 2).ToString();
            generationGapBox.Text = "1";

            MessageBox.Show("Лабиринт пройден!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = " ";
            textBox3.Text = " ";
            textBox4.Text = " ";
            textBox5.Text = " ";
            groupBox3.Visible = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[,] matrixVawe = copyMatrix(cellDataSet);
            prepareMatrixForVaweAlg(matrixVawe);
            matrixVawe[startPoint[0], startPoint[1]] = 1;
            int vaweAlgVal = 1;

            while (true)
            {
                if (!findTheWayWave(endPoint, vaweAlgVal, matrixVawe))
                    vaweAlgVal++;
                else
                    break;
            }
            Console.WriteLine(endPoint[0].ToString() + " " + endPoint[1].ToString());

            int wayValue = matrixVawe[endPoint[0], endPoint[1]];
            int[] currentCoordinates = new int[2] { endPoint[0], endPoint[1] };
            while (wayValue > 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        if (wayValue == matrixVawe[j, i])
                        {
                            if ((Math.Abs(j - currentCoordinates[0]) < 2 && Math.Abs(i - currentCoordinates[1]) == 0) ||
                                (Math.Abs(j - currentCoordinates[0]) == 0 && Math.Abs(i - currentCoordinates[1]) < 2))
                            {
                                matrixVawe[j, i] = 1000;
                                currentCoordinates[0] = j;
                                currentCoordinates[1] = i;
                                break;
                            }
                        }
                    }
                }

                refreshVaweTablePaint(matrixVawe, vaweAlgVal);
                wayValue--;
            }

            stopwatch.Stop();
            TimeSpan stopwatchEllapsed = stopwatch.Elapsed;
            int seconds = Convert.ToInt32(stopwatchEllapsed.TotalSeconds);
            int miliseconds = Convert.ToInt32(stopwatchEllapsed.TotalMilliseconds) % 100;
            textBox2.Text = seconds.ToString() + "," + miliseconds.ToString();

            int touchedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrixVawe[j, i] != -1 && matrixVawe[j, i] != 0)
                    {
                        touchedCells += 1;
                    }
                }
            }
            textBox3.Text = touchedCells.ToString();

            int abourtedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrixVawe[j, i] != 1000 && matrixVawe[j, i] != -1 && matrixVawe[j, i] != 0)
                    {
                        abourtedCells += 1;
                    }
                }
            }
            textBox4.Text = abourtedCells.ToString();

            int wayLength = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (matrixVawe[j, i] == 1000)
                    {
                        wayLength++;
                    }
                }
            }
            textBox5.Text = wayLength.ToString();

            MessageBox.Show("Лабиринт пройден!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = " ";
            textBox3.Text = " ";
            textBox4.Text = " ";
            textBox5.Text = " ";
            groupBox3.Visible = false;

            int[] currentPoint = new int[2] { endPoint[0], endPoint[1] };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            findTheWayTremaux(startPoint, endPoint, cellDataSet);
            thremauxSolving = false;

            while (true)
            {
                cellDataSet[currentPoint[0], currentPoint[1]] = 9;
                int y = currentPoint[0];
                int x = currentPoint[1];

                if (y == 11 && x == 3)
                {
                    Console.WriteLine("Тут!");

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();

                    for (int i = 0; i < 25; i++)
                    {
                        string str = "";

                        for (int j = 0; j < 25; j++)
                        {
                            str += cellDataSet[j, i].ToString();
                        }

                        Console.WriteLine(str);
                    }
                }

                if (x > 0)
                {
                    if (cellDataSet[y, x - 1] == 6)
                    {
                        cellDataSet[y, x - 1] = 9;
                        currentPoint[1] = x - 1;
                        refreshTremauxTablePaint(cellDataSet);
                        break;
                    }
                }
                if (x < 24)
                {
                    if (cellDataSet[y, x + 1] == 6)
                    {
                        cellDataSet[y, x + 1] = 9;
                        currentPoint[1] = x + 1;
                        refreshTremauxTablePaint(cellDataSet);
                        break;
                    }
                }
                if (y > 0)
                {
                    if (cellDataSet[y - 1, x] == 6)
                    {
                        cellDataSet[y - 1, x] = 9;
                        currentPoint[0] = y - 1;
                        refreshTremauxTablePaint(cellDataSet);
                        break;
                    }
                }
                if (y < 24)
                {
                    if (cellDataSet[y + 1, x] == 6)
                    {
                        cellDataSet[y + 1, x] = 9;
                        currentPoint[0] = y + 1;
                        refreshTremauxTablePaint(cellDataSet);
                        break;
                    }
                }

                if (x > 0)
                {
                    if (cellDataSet[y, x - 1] == 4 || cellDataSet[y, x - 1] == 8 || cellDataSet[y, x - 1] == 2 || cellDataSet[y, x - 1] == 6)
                    {
                        if (x > 1)
                        {
                            if (cellDataSet[y, x - 2] == 4 || cellDataSet[y, x - 2] == 8 || cellDataSet[y, x - 2] == 2 || cellDataSet[y, x - 2] == 6)
                            {
                                currentPoint[1] = x - 1;
                                refreshTremauxTablePaint(cellDataSet);
                                continue;
                            }
                        }

                        if (cellDataSet[y + 1, x - 1] == 4 || cellDataSet[y + 1, x - 1] == 8 || cellDataSet[y + 1, x - 1] == 2 || cellDataSet[y + 1, x - 1] == 6 ||
                            cellDataSet[y - 1, x - 1] == 4 || cellDataSet[y - 1, x - 1] == 8 || cellDataSet[y - 1, x - 1] == 2 || cellDataSet[y - 1, x - 1] == 6)
                        {
                            currentPoint[1] = x - 1;
                            refreshTremauxTablePaint(cellDataSet);
                            continue;
                        }
                    }
                }
                if (x < 24)
                {
                    if (cellDataSet[y, x + 1] == 4 || cellDataSet[y, x + 1] == 8 || cellDataSet[y, x + 1] == 2 || cellDataSet[y, x + 1] == 6)
                    {
                        if (x < 23)
                        {
                            if (cellDataSet[y, x + 2] == 4 || cellDataSet[y, x + 2] == 8 || cellDataSet[y, x + 2] == 2 || cellDataSet[y, x + 2] == 6)
                            {
                                currentPoint[1] = x + 1;
                                refreshTremauxTablePaint(cellDataSet);
                                continue;
                            }
                        }
                        if (cellDataSet[y + 1, x + 1] == 4 || cellDataSet[y + 1, x + 1] == 8 || cellDataSet[y + 1, x + 1] == 2 || cellDataSet[y + 1, x + 1] == 6 ||
                            cellDataSet[y - 1, x + 1] == 4 || cellDataSet[y - 1, x + 1] == 8 || cellDataSet[y - 1, x + 1] == 2 || cellDataSet[y - 1, x + 1] == 6)
                        {
                            currentPoint[1] = x + 1;
                            refreshTremauxTablePaint(cellDataSet);
                            continue;
                        }
                    }
                }
                if (y > 0)
                {
                    if (cellDataSet[y - 1, x] == 4 || cellDataSet[y - 1, x] == 8 || cellDataSet[y - 1, x] == 2 || cellDataSet[y - 1, x] == 6)
                    {
                        if (y > 1)
                        {
                            if (cellDataSet[y - 2, x] == 4 || cellDataSet[y - 2, x] == 8 || cellDataSet[y - 2, x] == 2 || cellDataSet[y - 2, x] == 6)
                            {
                                currentPoint[0] = y - 1;
                                refreshTremauxTablePaint(cellDataSet);
                                continue;
                            }
                        }
                        if (cellDataSet[y - 1, x + 1] == 4 || cellDataSet[y - 1, x + 1] == 8 || cellDataSet[y - 1, x + 1] == 2 || cellDataSet[y - 1, x + 1] == 6 ||
                            cellDataSet[y - 1, x - 1] == 4 || cellDataSet[y - 1, x - 1] == 8 || cellDataSet[y - 1, x - 1] == 2 || cellDataSet[y - 1, x - 1] == 6)
                        {
                            currentPoint[0] = y - 1;
                            refreshTremauxTablePaint(cellDataSet);
                            continue;
                        }
                    }
                }
                if (y < 24)
                {
                    if (cellDataSet[y + 1, x] == 4 || cellDataSet[y + 1, x] == 8 || cellDataSet[y + 1, x] == 2 || cellDataSet[y + 1, x] == 6)
                    {
                        if (y < 23)
                        {
                            if (cellDataSet[y + 2, x] == 4 || cellDataSet[y + 2, x] == 8 || cellDataSet[y + 2, x] == 2 || cellDataSet[y + 2, x] == 6)
                            {
                                currentPoint[0] = y + 1;
                                refreshTremauxTablePaint(cellDataSet);
                                continue;
                            }
                        }
                        if (cellDataSet[y + 1, x + 1] == 4 || cellDataSet[y + 1, x + 1] == 8 || cellDataSet[y + 1, x + 1] == 2 || cellDataSet[y + 1, x + 1] == 6 ||
                            cellDataSet[y + 1, x - 1] == 4 || cellDataSet[y + 1, x - 1] == 8 || cellDataSet[y + 1, x - 1] == 2 || cellDataSet[y + 1, x - 1] == 6)
                        {
                            currentPoint[0] = y + 1;
                            refreshTremauxTablePaint(cellDataSet);
                            continue;
                        }
                    }
                }
            }

            stopwatch.Stop();
            TimeSpan stopwatchEllapsed = stopwatch.Elapsed;
            int seconds = Convert.ToInt32(stopwatchEllapsed.TotalSeconds);
            int miliseconds = Convert.ToInt32(stopwatchEllapsed.TotalMilliseconds) % 100;
            textBox2.Text = seconds.ToString() + "," + miliseconds.ToString();

            int touchedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] != 1 && cellDataSet[j, i] != 0)
                    {
                        touchedCells += 1;
                    }
                }
            }
            textBox3.Text = touchedCells.ToString();

            int abourtedCells = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 3 || cellDataSet[j, i] == 5)
                    {
                        abourtedCells += 1;
                    }
                }
            }
            textBox4.Text = abourtedCells.ToString();

            int wayLength = 0;
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (cellDataSet[j, i] == 9)
                    {
                        wayLength += 1;
                    }
                }
            }
            textBox5.Text = wayLength.ToString();

            MessageBox.Show("Лабиринт пройден!");
        }
    }
}
