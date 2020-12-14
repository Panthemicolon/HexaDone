using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tasklib;

namespace HexaDone
{
    /// <summary>
    /// Interaktionslogik für TopicView.xaml
    /// </summary>
    public partial class TopicView : UserControl
    {
        private HSVColor topicColor;
        public event Action<TopicView, Task, bool> TaskStatusChanged;

        public string TopicName
        {
            get { return this.tblTopicName.Text; }
            set { this.tblTopicName.Text = value; }
        }

        public HSVColor TopicColor
        {
            get
            {
                return this.topicColor;
            }
            set
            {
                this.topicColor = value;
                (byte r, byte g, byte b) rgb = this.topicColor.ToRGB();
                this.brdMain.Background = new SolidColorBrush(Color.FromRgb(rgb.r, rgb.g, rgb.b));
            }
        }

        public int Tasks
        {
            get
            {
                int count = 0;
                foreach (UIElement elem in this.stkpnlTasks.Children)
                {
                    CheckBox checkBox = elem as CheckBox;
                    if (checkBox == null)
                        continue;

                    count++;
                }

                return count;
            }
        }

        public TopicView()
        {
            InitializeComponent();
            this.tblTopicName.VerticalAlignment = VerticalAlignment.Center;
        }

        internal void AddTask(Task task)
        {
            Task t = task;
            CheckBox checkBox = CreateCheckBox(t);
            stkpnlTasks.Children.Add(checkBox);
            if (this.stkpnlTasks.Children.Count > 1)
            {
                this.tblTopicName.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        private CheckBox CreateCheckBox(Task task)
        {
            Task t = task;
            CheckBox checkBox = new CheckBox();
            checkBox.Content = t.Content;
            checkBox.Foreground = t.Done ? Brushes.Gray : Brushes.Black;
            checkBox.IsChecked = t.Done;
            checkBox.Tag = t;
            checkBox.Checked += CheckBox_Checked;
            checkBox.Unchecked += CheckBox_Checked;
            return checkBox;
        }

        internal void RemoveTask(Task task)
        {
            Task t = task;

            CheckBox removeCheckbox = null;
            foreach (UIElement child in this.stkpnlTasks.Children)
            {
                CheckBox checkBox = child as CheckBox;
                if (checkBox != null && checkBox.Tag == t)
                {
                    removeCheckbox = checkBox;
                    break;
                }
            }

            if (removeCheckbox != null)
            {
                this.stkpnlTasks.Children.Remove(removeCheckbox);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                checkBox.Foreground = (checkBox.IsChecked ?? false ? Brushes.Gray : Brushes.Black);
                this.TaskStatusChanged?.Invoke(this, (Task)checkBox.Tag, checkBox.IsChecked ?? false);
            }
        }

        public void OrderTasks()
        {
            List<Task> tasks = new List<Task>();
            foreach (CheckBox checkBox in this.stkpnlTasks.Children)
            {
                tasks.Add((Task)checkBox.Tag);
            }
            tasks = tasks.OrderBy(t => t.Done).ThenBy(t => t.CreationTime).ToList();
            this.stkpnlTasks.Children.Clear();
            foreach (Task t in tasks)
            {
                this.stkpnlTasks.Children.Add(CreateCheckBox(t));
            }
        }
    }
}
