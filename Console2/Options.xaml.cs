using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Console2
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            Init_combobox();

            outputMin_textBox.Text = String.Format("{0:0.00}", Properties.Settings.Default.OptionsMin);
            outputMax_textBox.Text = String.Format("{0:0.00}", Properties.Settings.Default.OptionsMax);
            stepSize_textBox.Text = String.Format("{0:0}", Properties.Settings.Default.OptionsSteps);
            interval_textBox.Text = String.Format("{0:0}", Properties.Settings.Default.OptionsInterval);

            if (Properties.Settings.Default.OptionsCom != "")
            {
                adam_comboBox.Text = Properties.Settings.Default.OptionsCom;
            }
        }

        private void Init_combobox()
        {
            adam_comboBox.DisplayMemberPath = "Text";
            adam_comboBox.SelectedValuePath = "Value";

            var items = new[] {
                new { Text = "COM1", Value = "COM1" }, new { Text = "COM2", Value = "COM2" },
                new { Text = "COM3", Value = "COM3" }, new { Text = "COM4", Value = "COM4" },
                new { Text = "COM5", Value = "COM5" }, new { Text = "COM6", Value = "COM6" },
                new { Text = "COM7", Value = "COM7" }, new { Text = "COM8", Value = "COM8" },
                new { Text = "COM9", Value = "COM9" }, new { Text = "COM10", Value = "COM10" },
                new { Text = "COM11", Value = "COM11" }, new { Text = "COM12", Value = "COM12" },
                new { Text = "COM13", Value = "COM13" }, new { Text = "COM14", Value = "COM14" },
                new { Text = "COM15", Value = "COM15" }, new { Text = "COM16", Value = "COM16" },
                new { Text = "COM17", Value = "COM17" }, new { Text = "COM18", Value = "COM18" },
                new { Text = "COM19", Value = "COM19" }, new { Text = "COM20", Value = "COM20" },
                new { Text = "COM21", Value = "COM21" }, new { Text = "COM22", Value = "COM22" },
                new { Text = "COM23", Value = "COM23" }, new { Text = "COM24", Value = "COM24" },
                new { Text = "COM25", Value = "COM25" }, new { Text = "COM26", Value = "COM26" },
                new { Text = "COM27", Value = "COM27" }, new { Text = "COM28", Value = "COM28" },
                new { Text = "COM29", Value = "COM29" }, new { Text = "COM30", Value = "COM30" },
                new { Text = "COM31", Value = "COM31" }, new { Text = "COM32", Value = "COM32" },
                new { Text = "COM33", Value = "COM33" }, new { Text = "COM34", Value = "COM34" },
                new { Text = "COM35", Value = "COM35" }, new { Text = "COM36", Value = "COM36" },
                new { Text = "COM37", Value = "COM37" }, new { Text = "COM38", Value = "COM38" },
                new { Text = "COM39", Value = "COM39" }, new { Text = "COM40", Value = "COM40" },
                new { Text = "COM41", Value = "COM41" }, new { Text = "COM42", Value = "COM42" },
                new { Text = "COM43", Value = "COM43" }, new { Text = "COM44", Value = "COM44" },
                new { Text = "COM45", Value = "COM45" }, new { Text = "COM46", Value = "COM46" },
                new { Text = "COM47", Value = "COM47" }, new { Text = "COM48", Value = "COM48" },
                new { Text = "COM49", Value = "COM49" }, new { Text = "COM50", Value = "COM50" }
            };

            adam_comboBox.ItemsSource = items;
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            if (!boundaryCheck())
            {
                MessageBox.Show("Check input values.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Properties.Settings.Default.OptionsMin = Double.Parse(outputMin_textBox.Text);
                Properties.Settings.Default.OptionsMax = Double.Parse(outputMax_textBox.Text);
                Properties.Settings.Default.OptionsSteps = Double.Parse(stepSize_textBox.Text);
                Properties.Settings.Default.OptionsInterval = Double.Parse(interval_textBox.Text);
                Properties.Settings.Default.OptionsCom = adam_comboBox.Text;
                Properties.Settings.Default.Save();

                this.Close();
            }
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private Boolean boundaryCheck()
        {
            output_label.Foreground = System.Windows.Media.Brushes.Black;
            outputMin_label.Foreground = System.Windows.Media.Brushes.Black;
            outputMax_label.Foreground = System.Windows.Media.Brushes.Black;
            step_label.Foreground = System.Windows.Media.Brushes.Black;
            interval_label.Foreground = System.Windows.Media.Brushes.Black;
            adam_label.Foreground = System.Windows.Media.Brushes.Black;

            Boolean result = true;

            double _min, _max, _steps, _interval;
            _min = Double.Parse(outputMin_textBox.Text);
            _max = Double.Parse(outputMax_textBox.Text);
            _steps = Double.Parse(stepSize_textBox.Text);
            _interval = Double.Parse(interval_textBox.Text);

            if (!(_min >= -5.00 && _min <= 0.00))
            {
                result = false;
                outputMin_label.Foreground = System.Windows.Media.Brushes.Red;
                output_label.Foreground = System.Windows.Media.Brushes.Red;
            }
            if (!(_max >= -5.00 && _max <= 0.00))
            {
                result = false;
                outputMax_label.Foreground = System.Windows.Media.Brushes.Red;
                output_label.Foreground = System.Windows.Media.Brushes.Red;
            }
            if (_min >= _max)
            {
                result = false;
                outputMin_label.Foreground = System.Windows.Media.Brushes.Red;
                outputMax_label.Foreground = System.Windows.Media.Brushes.Red;
                output_label.Foreground = System.Windows.Media.Brushes.Red;
            }
            if (!(_steps >= 20 && _steps <= 100))
            {
                result = false;
                step_label.Foreground = System.Windows.Media.Brushes.Red;
            }
            if (!(_interval >= 100 && _interval <= 2000))
            {
                result = false;
                interval_label.Foreground = System.Windows.Media.Brushes.Red;
            }
            if (adam_comboBox.Text == "")
            {
                result = false;
                adam_label.Foreground = System.Windows.Media.Brushes.Red;
            }

            return result;
        }
    }
}
