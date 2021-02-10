// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using Lighting;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private float red = 0.5f;
        private float green = 0.5f;
        private float blue = 0.5f;
        private float alpha = 1f;

        private bool animation = false;
        private bool automatic_animation = true;

        private float scaling = 1f;
        private float goal_distance = 0f;
        private float animation_speed = 1f;
        private float ball_x = 0f;
        private float ball_y = 0f;
        private float ball_z = 0f;
        private float ball_rotation = 0f;
        private bool ball_down = true;

        private enum TextureObjects { Plastic = 0, Grass };
        private string[] m_textureFiles = { "..//..//images//plastic.jpg", "..//..//images//grass.jpg" };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;


        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        public float Red { get => red; set => red = value; }
        public float Green { get => green; set => green = value; }
        public float Blue { get => blue; set => blue = value; }
        public float Alpha { get => alpha; set => alpha = value; }

        public bool Animation { get => animation; set => animation = value; }
        public bool AutomaticAnimation { get => automatic_animation; set => automatic_animation = value; }

        public float Scaling { get => scaling; set => scaling = value; }

        public float GoalDistance { get => goal_distance; set => goal_distance = value; }

        public float AnimationSpeed { get => animation_speed; set => animation_speed = value; }

        public float BallX { get => ball_x; set => ball_x = value; }

        public float BallY { get => ball_y; set => ball_y = value; }

        public float BallZ { get => ball_z; set => ball_z = value; }

        public float BallRotation { get => ball_rotation; set => ball_rotation = value; }

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.75f, 0.8f, 0.95f, 0.5f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_CULL_FACE); 
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.FrontFace(OpenGL.GL_CW);

            #region 1_COLOR_TRACKING
            //Mehanizam za color tracking
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_NORMALIZE);
            #endregion

            #region 2_TACKASTI_IZVOR_SVETLOSTI
            //Tackasti izvor svetlosti
            float[] light0pos = new float[] { 30000f, 0f, 30f, 1f };
            float[] light0ambient = new float[] { 0.5f, 0.5f, 0.5f, alpha };
            float[] light0diffuse = new float[] { 0.35f, 0.35f, 0.35f, 1.0f };
            float[] light0specular = new float[] { 0.5f, 0.5f, 0.5f, 1f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Enable(OpenGL.GL_LIGHT0);
            #endregion

            #region 9_REFLEKTORSKI_SVETLOSNI_IZVOR
            //Reflektorski izvor svetlosti
            float[] light1pos = new float[] { 5f, 0f, 5f, 1f };
            float[] light1ambient = new float[] { 1f, 0f, 1f, 1.0f };
            float[] light1diffuse = new float[] { 1f, 0.0f, 1f, 1.0f };
            float[] light1specular = new float[] { 1f, 0f, 1f, 1.0f };
            float[] light1direction = new float[] { 0f, -1f, 0f, 0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light1direction);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Enable(OpenGL.GL_LIGHT1);
            #endregion

            #region 3_DODAVANJE_TEKSTURA
            //TEKSTURA
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            
            #region 3_B_STAPANJE_TEKSTURE_ADD
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            #endregion
            
            // Ucitavanje slika i pravljenje tekstura
            gl.GenTextures(m_textureCount, m_textures);
            
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Console.WriteLine(m_textureFiles[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                #region 3_A_WRAPPING_REPEAT_I_NEAREST_FILTER
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                #endregion

                image.UnlockBits(imageData);
                image.Dispose();
            }
            #endregion

            #region 5_SKALIRANJA_TEKSTURA
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.Scale(5f, 5f, 5f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            #endregion

            m_scene.LoadScene();
            m_scene.Initialize();

        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.LoadIdentity();

            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            #region 6_POZICIONIRANJE_KAMERE
            gl.LookAt(0, 2, 2, 0, 0, -m_sceneDistance, 0, 1, 0);
            #endregion

            #region 2_AUTOMATSKO_GENERISANJE_NORMALA
            gl.Enable(OpenGL.GL_AUTO_NORMAL);
            #endregion

            gl.PushMatrix();
            gl.Translate(0.0f, -260f, -m_sceneDistance);
            gl.Scale(5f, 5f, 5f);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            #region Podloga
            gl.PushMatrix();

            gl.Translate(100f, 1500f, -160f);
            gl.Rotate(3, 0, 0);
            //gl.Color(0.19f, 0.415f, 0.22f);
            gl.Color(0.2f, 0.2f, 0.2f);
            gl.Scale(7f, 5f, 5f);

            /*gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(-200f, -320f, -1800f); //-65f, -300f, -1400f
            gl.Vertex(200f, -320f, -1800f); //65f, -300f, -1400f
            gl.Vertex(200f, -320f, 1800f);  //65f, -300f, 1600f
            gl.Vertex(-200f, -320f, 1800f); //-65f, -300f, 1600f
            gl.End();*/

            #region 5_DODAVANJE_TEKSTURE_TRAVE
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(LightingUtilities.FindFaceNormal(5f, 0f, 1f, -5f, 0f, 1f, -5f, 0f, -10f));
            gl.TexCoord(1f, 0f);
            gl.Vertex4f(-200f, -320f, -1800f, 1);

            gl.TexCoord(0f, 0f);
            gl.Vertex4f(200f, -320f, -1800f, 1);

            gl.TexCoord(1f, 1f);
            gl.Vertex4f(200f, -320f, 1800f, 1);

            gl.TexCoord(0f, 1f);
            gl.Vertex4f(-200f, -320f, 1800f, 1);

            gl.End();
            #endregion

            gl.PopMatrix();
            #endregion

            #region Gol

            gl.PushMatrix();

            #region 10_GL_MODULATE_GOL
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            #endregion
            gl.Scale(1f, 1f, 1f);
            gl.Translate(-315f, 500f, -240f + goal_distance);
            gl.Rotate(90f, 90f, 0f);
            gl.Color(1f, 1f, 1f);

            #region 4_DODAVANJE_TEKSTURE_PLASTIKE_GOL
            //Plastic texture
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            Cylinder cylinder = new Cylinder();
            cylinder.BaseRadius = 5;
            cylinder.TopRadius = 5;
            cylinder.Height = 400;

            cylinder.CreateInContext(gl);

            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;

            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            #endregion

            gl.PopMatrix();

            #endregion

            #region Levi deo
            gl.PushMatrix();

            #region 10_GL_MODULATE_LEVI_DEO
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            #endregion
            gl.Scale(1f, 1f, 1f);
            gl.Translate(-310f, 900f, -240f + goal_distance);
            gl.Rotate(90f, 0f, 0f);
            gl.Color(1f, 1f, 1f);

            #region 4_DODAVANJE_TEKSTURE_PLASTIKE_LEVI_DEO
            //Plastic texture
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            Cylinder cylinderleft = new Cylinder();
            cylinderleft.BaseRadius = 7;
            cylinderleft.TopRadius = 7;
            cylinderleft.Height = 1000;


            cylinderleft.CreateInContext(gl);

            cylinderleft.TextureCoords = true;
            cylinderleft.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            cylinderleft.NormalOrientation = Orientation.Outside;

            cylinderleft.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            #endregion

            gl.PopMatrix();

            #endregion

            #region Desni deo
            gl.PushMatrix();

            #region 10_GL_MODULATE_DESNI_DEO
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            #endregion
            gl.Scale(1f, 1f, 1f);
            gl.Translate(90f, 900f, -240f + goal_distance);
            gl.Rotate(90f, 0f, 0f);
            gl.Color(1f, 1f, 1f);

            #region 4_DODAVANJE_TEKSTURE_PLASTIKE_DESNI_DEO
            //Plastic texture
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            Cylinder cylinderright = new Cylinder();
            cylinderright.BaseRadius = 7;
            cylinderright.TopRadius = 7;
            cylinderright.Height = 1000;
            cylinderright.TextureCoords = true;

            cylinderright.CreateInContext(gl);

            cylinderright.TextureCoords = true;
            cylinderright.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            cylinderright.NormalOrientation = Orientation.Outside;

            cylinderright.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            #endregion

            gl.PopMatrix();

            #endregion

            #region Levi deo iza
            gl.PushMatrix();

            #region 10_GL_MODULATE_LEVI_DEO_IZA
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            #endregion
            gl.Scale(1f, 1f, 1f);
            gl.Translate(-310f, 290f, -240f + goal_distance);
            gl.Rotate(135f, 0f, 0f);
            gl.Color(1f, 1f, 1f);

            #region 4_DODAVANJE_TEKSTURE_PLASTIKE_LEVI_DEO_IZA
            //Plastic texture
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            Cylinder cylinderleftback = new Cylinder();
            cylinderleftback.BaseRadius = 7;
            cylinderleftback.TopRadius = 7;
            cylinderleftback.Height = 700;
            cylinderleftback.TextureCoords = true;

            cylinderleftback.CreateInContext(gl);

            cylinderleftback.TextureCoords = true;
            cylinderleftback.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            cylinderleftback.NormalOrientation = Orientation.Outside;

            cylinderleftback.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            #endregion

            gl.PopMatrix();

            #endregion

            #region Desni deo iza
            gl.PushMatrix();

            #region 10_GL_MODULATE_DESNI_DEO_IZA
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            #endregion
            gl.Scale(1f, 1f, 1f);
            gl.Translate(90f, 290f, -240f + goal_distance);
            gl.Rotate(135f, 0f, 0f);
            gl.Color(1f, 1f, 1f);

            #region 4_DODAVANJE_TEKSTURE_PLASTIKE_DESNI_DEO_IZA
            //Plastic texture
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            Cylinder cylinderrightback = new Cylinder();
            cylinderrightback.BaseRadius = 7;
            cylinderrightback.TopRadius = 7;
            cylinderrightback.Height = 700;
            cylinderrightback.TextureCoords = true;
            cylinderrightback.CreateInContext(gl);

            cylinderrightback.TextureCoords = true;
            cylinderrightback.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            cylinderrightback.NormalOrientation = Orientation.Outside;

            cylinderrightback.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            #endregion

            gl.PopMatrix();

            #endregion


            #region 7_SKALIRANJE_LOPTE
            gl.Scale(scaling * 0.2f, scaling * 0.2f, scaling * 0.2f);
            #endregion

            #region 11_AUTOMATSKO_ODSKAKANJE_I_ROTACIJA_LOPTE
            if (automatic_animation)
            {
                if (ball_down)
                {
                    ball_y -= 100f;
                    if (ball_y < -570f)
                    {
                        ball_down = false;
;                    }
                } else
                {
                    ball_y += 100f;
                    if (ball_y > 0f)
                    {
                        ball_down = true;
                    }
                }
                gl.Translate(0f, ball_y, 0f); //-570f
                ball_rotation += 10f;
                gl.Rotate(ball_rotation, 0f, 0f);
            }
            #endregion

            #region 12_ANIMACIJA_LOPTA_U_GOL
            if (animation)
            {
                if (ball_z < 0f && ball_z > -1000f)
                {
                    ball_z -= (100f * animation_speed);
                    ball_y = (-7f / 2f) * ball_z;
                } else if (ball_z > -3000f)
                {
                    ball_z -= (100f * animation_speed);
                    ball_y = (7f / 4f) * ball_z + 5250f;
                } else
                {
                    animation = false;
                }
                gl.Translate(0f, ball_y, ball_z);
            }
            #endregion

            m_scene.Draw();
            gl.PopMatrix();

            #region Tekst
            gl.PushMatrix();

            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-1.0, 1.0, 0, 1.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Color(1f, 0f, 0f);
            gl.Scale(0.04, 0.04, 0.04);
            String[] msg = { "Predmet: Racunarska grafika", "Sk.god: 2020/21", "Ime: Vanja", "Prezime: Stanojevic", "Sifra zad: 7.2" };
            for (int i = 0; i < msg.Length; i++)
            {
                gl.PushMatrix();
                gl.Translate(12.0f, 5 - i, 0f);
                gl.DrawText3D("Tahoma", 10f, 1f, 0.1f, msg[i]);
                gl.PopMatrix();
            }
            gl.PopMatrix();
            gl.PopMatrix();
            #endregion

            #region TekstUn
            gl.PushMatrix();

            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-1.0, 1.0, 0, 1.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Color(1f, 0f, 0f);
            gl.Scale(0.04, 0.04, 0.04);
            String[] msg1 = { "________________________", "_______________", "___________", "_________________", "____________" };
            for (int i = 0; i < msg1.Length; i++)
            {
                gl.PushMatrix();
                gl.Translate(12.0f, 4.9 - i, 0f);
                gl.DrawText3D("Tahoma", 10f, 1f, 0.1f, msg1[i]);
                gl.PopMatrix();
            }
            gl.PopMatrix();
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (float)m_width / m_height, 1.0f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PopMatrix();
            gl.PopMatrix();
            #endregion

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.1f, 20000f);
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
