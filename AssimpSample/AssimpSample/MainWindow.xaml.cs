using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Touareg"), "football.3ds", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.Width, (int)openGLControl.Height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F10: this.Close(); break;
                case Key.E:
                    if ((m_world.RotationX < 90.0f) && m_world.Animation != true)
                    {
                        m_world.RotationX += 5.0f;

                    }
                    break;
                case Key.D:
                    if ((m_world.RotationX > 0.0f) && m_world.Animation != true)
                    {
                        m_world.RotationX -= 5.0f;
                    }
                    break;
                case Key.S: 
                    if (m_world.Animation != true) {
                        m_world.RotationY += 5.0f;
                    } 
                    break;
                case Key.F:
                    if (m_world.Animation != true)
                    {
                        m_world.RotationY -= 5.0f;
                    }
                    break;
                case Key.Add:
                    if (m_world.Animation != true)
                    {
                        m_world.SceneDistance -= 700.0f;
                    }
                    break;
                case Key.Subtract:
                    if (m_world.Animation != true)
                    {
                        m_world.SceneDistance += 700.0f;
                    }
                    break;
                case Key.F2: Environment.Exit(0); break;
                case Key.F4:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool) opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                        }
                    }
                    break;
                case Key.V:
                    m_world.BallX = 0f;
                    m_world.BallY = 0f;
                    m_world.BallZ = 0f;
                    m_world.Animation = true;
                    m_world.AutomaticAnimation = false;
                    break;
                case Key.B:
                    m_world.Animation = false;
                    m_world.AutomaticAnimation = true;
                    break;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void scaling_TextChanged(object sender, TextChangedEventArgs e)
        {
            double broj;

            if (Double.TryParse(scaling.Text, out broj) && m_world.Animation != true)
            {
                m_world.Scaling = (float)broj;
            }
            else
            {
                m_world.Scaling = 1f;
                scaling.Text = "";
            }

        }

        private void goal_distance_TextChanged(object sender, TextChangedEventArgs e)
        {
            double broj;

            if (Double.TryParse(goal_distance.Text, out broj) && m_world.Animation != true)
            {
                m_world.GoalDistance = (float)broj;
            }
            else
            {
                m_world.GoalDistance = 0f;
                scaling.Text = "";
            }

        }

        private void animation_speed_TextChanged(object sender, TextChangedEventArgs e)
        {
            double broj;

            if (Double.TryParse(animation_speed.Text, out broj) && m_world.Animation != true)
            {
                m_world.AnimationSpeed = (float)broj;
            }
            else
            {
                m_world.AnimationSpeed = 1f;
                scaling.Text = "";
            }

        }
    }
}
