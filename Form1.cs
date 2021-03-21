using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Task_1
{
    public partial class Form1 : Form
    {
        DataGridView dataGridView1 = new DataGridView();
        Bitmap image;
        MathMorfology MorfologyFilter;
        Label lrow, lcol;
        TextBox rows, cols;
        Button but, but_grid;
        int colCount = -1, rowCount = -1;

        public Form1()
        {
            InitializeComponent();
            CreateRequster();
        }

        private void CreateRequster()
        {
            //label row
            lrow = new Label();
            lrow.Text = "Количество строк :";
            lrow.Size = new Size(200, 20);
            lrow.Location = new Point(10, 50);
            lrow.BorderStyle =  BorderStyle.FixedSingle;
            //label col
            lcol = new Label();
            lcol.Text = "Количество столбцов :";
            lcol.Size = lrow.Size;
            lcol.Location = new Point(lrow.Location.X + lrow.Size.Width, lrow.Location.Y);
            lcol.BorderStyle = BorderStyle.FixedSingle;
            //TextBox row
            rows = new TextBox();
            rows.Location = new Point(lrow.Location.X, lrow.Location.Y + lrow.Size.Height);
            rows.Size = lrow.Size;
            //TextBox col
            cols = new TextBox();
            cols.Location = new Point(rows.Location.X + rows.Size.Width, rows.Location.Y);
            cols.Size = rows.Size;
            // button ok
            but = new Button();
            but.Location = new Point(lrow.Location.X, rows.Location.Y + rows.Height);
            but.Size = new Size(2*cols.Size.Width, cols.Size.Height);
            but.Text = "Принять";
            but.Click += new EventHandler(but_Click);
        }

        private void but_Click(object sender, EventArgs e)
        {
            rowCount = Convert.ToInt32(rows.Text);
            colCount = Convert.ToInt32(cols.Text);
            HideRequster();
            addGrid();
        }

        private void ShowRequster()
        {
            this.Controls.Add(but);
            this.Controls.Add(lcol);
            this.Controls.Add(lrow);
            this.Controls.Add(cols);
            this.Controls.Add(rows);
            pictureBox1.SendToBack();
            this.Refresh();
        }

        private void HideRequster()
        {
            this.Controls.Remove(lcol);
            this.Controls.Remove(lrow);
            this.Controls.Remove(cols);
            this.Controls.Remove(rows);
            this.Controls.Remove(but);
            this.Refresh();
        }
        void HideGrid()
        {
            this.Controls.Remove(dataGridView1);
            this.Controls.Remove(but_grid);
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            this.Refresh();
        }
        void addGrid()
        {
            dataGridView1.Location = new Point(0,23);
            dataGridView1.Size = new Size(400, 200);
            dataGridView1.AutoSize = true;
            for (int i=0; i<colCount; i++)
            {
                DataGridViewColumn col = new DataGridViewColumn();
                col.CellTemplate = new DataGridViewTextBoxCell();
                col.Width = (int)(dataGridView1.Size.Width / colCount);
                dataGridView1.Columns.Add(col);
            }
            for (int i = 0; i < rowCount; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewCell[] cells= new DataGridViewCell[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    cells[j] = new DataGridViewTextBoxCell();
                    cells[j].Value = "1";
                }
                row.Height = (int)(dataGridView1.Size.Height / rowCount);
                row.Cells.AddRange(cells);
                dataGridView1.Rows.Add(row);
            }
            
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Visible = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;
            but_grid = new Button();
            but_grid.Location = new Point(dataGridView1.Location.X, dataGridView1.Location.Y + 
                dataGridView1.Size.Height);
            but_grid.Text = "Принять";
            but_grid.Size = new Size(dataGridView1.Size.Width, 30);
            but_grid.Click += new EventHandler(but_grid_click);
            this.Controls.Add(but_grid);
            this.Controls.Add(dataGridView1);
            this.pictureBox1.SendToBack();
            this.Refresh();
        }

        private void but_grid_click(object sender, EventArgs e)
        {
            int[,] a = new int[rowCount, colCount];
            for (int i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; ++j)
                {
                    a[i,j] = Convert.ToInt32(dataGridView1[j, i].Value.ToString());
                }
            }
            MorfologyFilter.setMask(a);
            HideGrid();
            backgroundWorker1.RunWorkerAsync(MorfologyFilter);
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            if (open.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(open.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image,backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void полутонToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new AcutanceFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void волныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WavesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GreyWorldFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеГистограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LinearHistogramFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void медианныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRequster();
            MorfologyFilter = new DilationFilter();
        }

        private void ярчеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new IntensityFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRequster();
            MorfologyFilter = new ErosionFilter();
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRequster();
            MorfologyFilter = new OpeningFilter();
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRequster();
            MorfologyFilter = new ClosingFilter();
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRequster();
            MorfologyFilter = new GradFilter();
        }
    }
}
