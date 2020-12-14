using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tasklib;

#nullable enable
namespace HexaDone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string DatabaseFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "");
        private readonly string NoneTopicName = "General";
        private readonly string TopicPattern = @"#(\S+): ";

        private TaskManager TaskManager { get; set; }

        private Dictionary<string, HSVColor> TopicColors { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.TaskManager = new TaskManager();
            this.TopicColors = new Dictionary<string, HSVColor>();
        }

       private Tasklib.Task? Parse(string text)
        {
            string textcontent = text;
            if (string.IsNullOrWhiteSpace(textcontent))
            {
                return null;
            }

            Match match = Regex.Match(textcontent, this.TopicPattern);
            if (match.Success && match.Index == 0)
            {
                return this.TaskManager.CreateTask(textcontent.Substring(match.Length), match.Groups[1].Value);
            }
            else
            {
                return this.TaskManager.CreateTask(textcontent, string.Empty);
            }
        }

        private void EnsureEnvironmentSetup()
        {
            // Make sure the Directory for the Save Files exists
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(this.DatabaseFilePath)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.DatabaseFilePath));
        }

        private bool Load()
        {
            try
            {
                this.EnsureEnvironmentSetup();

                TaskManager? tm = FileManager.LoadTaskManager(Path.Combine(this.DatabaseFilePath, "tasks.json"));
                if (tm != null)
                {
                    this.TaskManager = tm;
                }
                else
                {
                    this.TaskManager = new TaskManager();
                }

                this.TopicColors = FileManager.LoadTopicColors(Path.Combine(this.DatabaseFilePath, "topiccolors.json"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool Save()
        {
            try
            {
                this.EnsureEnvironmentSetup();

                bool success = FileManager.SaveTasks(Path.Combine(this.DatabaseFilePath, "tasks.json"), this.TaskManager);
                success &= FileManager.SaveTopicColors(Path.Combine(this.DatabaseFilePath, "topiccolors.json"), this.TopicColors);
                return success;
            }
            catch
            {
                return false;
            }
        }

        private void tbNewTask_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox? tb = sender as TextBox;
                if (tb == null || string.IsNullOrWhiteSpace(tb.Text))
                {
                    e.Handled = true;
                    return;
                }

                string text = tb.Text;
                Tasklib.Task? t = this.Parse(text);
                if (t != null)
                {
                    TopicView2 topicView = GetTopicView(stkpnlOpenTasks.Children, t.Topic);
                    topicView.AddTask(t);
                    topicView.OrderTasks();
                }
                tb.Clear();
                this.Save();

                e.Handled = true;
            }
        }

        private void TopicView_TaskStatusChanged(TopicView2 topicView, Tasklib.Task task, bool newStatus)
        {
            if (newStatus != task.Done)
            {
                // Remove the Task from the TopicView2 it was in
                topicView.RemoveTask(task);
                if (newStatus) // status is done, was open
                {
                    task.SetDone();
                    MoveTask(task, stkpnlCompletedTasks);
                    if (topicView.Tasks == 0)
                        stkpnlOpenTasks.Children.Remove(topicView);
                }
                else
                {
                    task.SetUndone();
                    MoveTask(task, stkpnlOpenTasks);
                    if (topicView.Tasks == 0)
                        stkpnlCompletedTasks.Children.Remove(topicView);
                }

                this.Save();
            }
            topicView.OrderTasks();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Load();
                this.InitializeTaskList();
            }
            catch (Exception ex)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("There was an error loading the Database:\r\n" + ex.Message + "\r\nWould you like to create a new Database?", "Error Loading Database", MessageBoxButton.YesNo);
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    if (this.TaskManager == null)
                    {
                        this.TaskManager = new TaskManager();
                    }
                    if (this.TopicColors == null)
                    {
                        this.TopicColors = new Dictionary<string, HSVColor>();
                    }
                }
            }
        }

        private void InitializeTaskList()
        {
            this.FillTasklist(stkpnlOpenTasks, false);
            this.FillTasklist(stkpnlCompletedTasks, true);
        }

        #region CreateMethods
        private TopicView2 CreateTopicView(string topicname, HSVColor topicColor)
        {
            TopicView2 topicView = new TopicView2();
            topicView.Margin = new Thickness(0, 0, 0, 10);
            topicView.TopicName = topicname;
            topicView.TopicColor = topicColor;
            topicView.TaskStatusChanged += TopicView_TaskStatusChanged;
            return topicView;
        }

        private HSVColor GetTopicColor(string topicname)
        {
            if (this.TopicColors.ContainsKey(topicname))
                return this.TopicColors[topicname];
            else if (Topic.None.Equals(topicname))
            {
                Color col = Colors.Gray;
                return HSVColor.FromRGB(col.R, col.G, col.B);
            }
            else
            {
                return ColorHelper.GetRandomHSVColor(false);
            }
        }
        #endregion

        #region UI_Tasklist_Helpers

        private void FillTasklist(StackPanel stackPanel, bool done)
        {
            stackPanel.Children.Clear();
            foreach (Topic topic in this.TaskManager.Topics)
            {
                string topicname = Topic.None.Equals(topic) ? this.NoneTopicName : topic.Name;
                Task[] tasks = this.TaskManager.GetTasksByTopic(topic.Name).Where(t => { return t.Done == done; }).ToArray();
                if (tasks.Length == 0)
                    continue;

                // Sort by Completed Date
                tasks = tasks.OrderBy(t => done ? t.CompletionTime : t.CreationTime).ToArray();
                TopicView2 topicView = CreateTopicView(topicname, GetTopicColor(topicname));

                if (!this.TopicColors.ContainsKey(topicname))
                {
                    this.TopicColors.Add(topicname, topicView.TopicColor);
                }

                foreach (Tasklib.Task task in tasks)
                {
                    topicView.AddTask(task);
                }

                stackPanel.Children.Add(topicView);
            }
        }

        /// <summary>
        /// Move the given Task to the <paramref name="target"/> StackPanel
        /// </summary>
        /// <param name="task"></param>
        /// <param name="target"></param>
        private void MoveTask(Tasklib.Task task, StackPanel target)
        {
            string topicname = (Topic.None.Equals(task.Topic) ? this.NoneTopicName : task.Topic);
            TopicView2? completedTopicView = GetTopicView(target.Children, topicname);
            completedTopicView.AddTask(task);
        }

        /// <summary>
        /// Get a TopicView2 for <paramref name="topicname"/> in <paramref name="elementCollection"/>
        /// </summary>
        /// <param name="elementCollection">The Element Collection containing the <see cref="TopicView2"/></param>
        /// <param name="topicname">The name of the Topic to get a <see cref="TopicView2"/></param>
        /// <returns></returns>
        private TopicView2 GetTopicView(UIElementCollection elementCollection, string topic)
        {
            string topicName = Topic.None.Equals(topic.ToLower().Trim()) ? this.NoneTopicName : topic.Trim();

            foreach (UIElement? view in elementCollection)
            {
                TopicView2? topicView = view as TopicView2;
                if (topicView == null)
                {
                    continue;
                }

                if (topicView.TopicName.ToLower().Trim() == topicName.ToLower())                  
                {
                    return topicView;
                }
            }


            // If we didn't find a TopicView2, Create one and add it to the given ElementCollection
            TopicView2 tView = CreateTopicView(topicName, GetTopicColor(topicName));
            elementCollection.Add(tView);
            if (!this.TopicColors.ContainsKey(topicName))
            {
                this.TopicColors.Add(topicName, tView.TopicColor);
            }
            return tView;
        }
        #endregion
    }
}
