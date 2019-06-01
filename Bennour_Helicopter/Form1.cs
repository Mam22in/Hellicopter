                            using System;
                            using System.Collections.Generic;
                            using System.ComponentModel;
                            using System.Data;
                            using System.Drawing;
                            using System.Drawing.Imaging;
                            using System.IO;
                            using System.Text;
                            using System.Windows.Forms;
                            using OpenTK;
                            using OpenTK.Graphics.OpenGL;
                            using System.Diagnostics;

                            namespace Bennour_Helicopter
                            {
                                public partial class Form1 : Form
                                {
                                    private static int COIN_NUMBER = 10;
                                    private static float COIN_RADIUS = 0.5f;
                                    private static float COIN_WIDTH = 0.2f;
                                    private static int TEXTURE_NUMBER = 3;

                                    bool loaded = false;
                                    float eyeX = 1.0f;
                                    float eyeY = 6.0f;
                                    float eyeZ = 1.5f;

                                    float directionX = 0.0f;
                                    float directionY = 0.0f;
                                    float directionZ = 0.0f;

                                    float rotation = 0;
                                    Bitmap map = null;
                                    int[] Textures = new int[TEXTURE_NUMBER];

                                    Coin[] coins = new Coin[COIN_NUMBER];
                                    int collectedCoinsCount = 0;
                                    
                                    public Form1()
                                    {
                                        InitializeComponent();
                                    }

                                    private void glControl1_Load(object sender, EventArgs e)
                                    {
                                        loaded = true;
                                        SetupViewport();
                                        setupLighting();
                                        Application.Idle += Application_Idle;
                                        Textures[0] =loadTexture("texture_UNN.bmp");
                                        Textures[1] =loadTexture("cent.jpg");
                                        Random R = new Random();
                                        for (int i = 0; i < COIN_NUMBER; i++ )
                                        {
                                            Vector3 position = new Vector3(R.Next(20) - 10,R.Next(20)-10,R.Next(2));
                                            Vector3 normal = new Vector3(R.Next(), R.Next(), 0);
                                            coins[i] = new Coin(COIN_RADIUS, COIN_WIDTH, position, normal);
                                        }
                                        map = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\heightmap.jpg");
                                    }

                                    void Application_Idle(Object sender, EventArgs e) 
                                    {
                                        while (glControl1.IsIdle)
                                        {
                                            rotation = (rotation + 0.1f) % 360;
                                            Render();
                                        }
                                    }

                                    private void SetupViewport()
                                    {
                                        GL.ClearColor(Color.RoyalBlue);
                                        GL.ShadeModel(ShadingModel.Smooth);
                                        GL.Enable(EnableCap.DepthTest);

                                        int viewportWidth = glControl1.Width;
                                        int viewportHeight = glControl1.Height;

                                        Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                                            MathHelper.PiOver4,
                                            viewportWidth / (float)viewportHeight, 
                                            1, 
                                            64);

                                        GL.MatrixMode(MatrixMode.Projection);
                                        GL.LoadMatrix(ref perspective);
                                    }

                                    private void glControl1_Paint(object sender, PaintEventArgs e)
                                    {
                                        Render();
                                        TouchCoin();
                                    }

                                    private void TouchCoin()
                                    {
                                        Vector3 spherePosition = new Vector3(directionX, directionY, directionZ);
                                        for (int i = 0; i < COIN_NUMBER; i++) 
                                        {
                                            if ((coins[i].center - spherePosition).Length < coins[i].radius + 1) 
                                            {
                                                Random R = new Random();
                                                Vector3 position = new Vector3(R.Next(40) - 20, R.Next(40) - 20, R.Next(8)-3);
                                                Vector3 normal = new Vector3(R.Next(), R.Next(), 0);
                                                coins[i] = new Coin(COIN_RADIUS, COIN_WIDTH, position, normal);
                                                collectedCoinsCount++;
                                                label1.Text = "Collected Coins: " + collectedCoinsCount;
                                            }
                                        }
                                    }

                                    private void Render()
                                    {
                                        if (!loaded) 
                                            return;
                                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                                        Matrix4 projectionMat = Matrix4.LookAt(eyeX, eyeY, eyeZ,
                                                                directionX, directionY, directionZ,
                                                                0, 0, 1);
                                        GL.MatrixMode(MatrixMode.Modelview);
                                        GL.LoadMatrix(ref projectionMat);
                                        drawLandScape(0, 0, -7, 1);
                                        //DrawGrid(Color.Green, 0, 0, -1, 1, 256);
                                        GL.Color3(Color.Yellow);
                                        GL.Enable(EnableCap.Texture2D);
                                        GL.BindTexture(TextureTarget.Texture2D, Textures[1]);
                                        for (int i = 0; i < COIN_NUMBER; i++ )
                                            drawCylinder(coins[i].radius, coins[i].width, coins[i].center, coins[i].normal, 20, 1);
                                        GL.Disable(EnableCap.Texture2D);
                                        GL.Translate(directionX, directionY, directionZ);
                                        drawFigure();
                                        glControl1.SwapBuffers();
                                    }
                                    
                                    Color calculateColor(int value)
                                    {
                                        if (value < 50)
                                            return Color.FromArgb(255 ,0 , 0);
                                        else if (value < 100)
                                            return Color.FromArgb(value * 2 + 50, value * 2 + 30, value);
                                        else if (value > 200)
                                            return Color.FromArgb(255, 255, 255);
                                        return Color.FromArgb(value / 3 * 2, value, value / 2);
                                    }
                                    public void DrawGrid(System.Drawing.Color color, float X, float Y, float Z, int cell_size = 1, int grid_size = 256)
                                    {
                                        int dX = (int)Math.Round(X / cell_size) * cell_size;
                                        int dY = (int)Math.Round(Y / cell_size) * cell_size;

                                        int ratio = grid_size / cell_size;

                                        GL.PushMatrix();

                                        GL.Translate(dX - grid_size / 2, dY - grid_size / 2, 0);

                                        int i;

                                        GL.Color3(color);
                                        GL.Begin(BeginMode.Lines);

                                        for (i = 0; i < ratio + 1; i++) 
                                        {
                                            int current = i * cell_size;
                                            GL.Vertex3(current, 0, Z);
                                            GL.Vertex3(current, grid_size, Z);

                                            GL.Vertex3(0, current, Z);
                                            GL.Vertex3(grid_size, current, Z);
                                        }

                                        GL.End();

                                        GL.PopMatrix();

                                    }
                                    void sphere(double r, int nx,int ny) 
                                    {
                                        int ix, iy;
                                        double x, y, z;
                                        for (iy = 0; iy < ny; ++iy) 
                                        {
                                            GL.Begin(BeginMode.QuadStrip);
                                            for (ix = 0; ix <= nx; ++ix)
                                            {
                                                x = r * Math.Sin(iy * Math.PI / ny) * Math.Cos(2 * ix * Math.PI / nx);
                                                y = r * Math.Sin(iy * Math.PI / ny) * Math.Sin(2 * ix * Math.PI / nx);
                                                z = r * Math.Cos(iy * Math.PI / ny);
                                                GL.Normal3(x, y, z);
                                                GL.TexCoord2((double)ix / (double)nx, (double)iy / (double)ny);
                                                GL.Vertex3(x, y, z);

                                                x = r * Math.Sin((iy+1) * Math.PI / ny) * Math.Cos(2 * ix * Math.PI / nx);
                                                y = r * Math.Sin((iy + 1) * Math.PI / ny) * Math.Sin(2 * ix * Math.PI / nx);
                                                z = r * Math.Cos((iy + 1) * Math.PI / ny);
                                                GL.Normal3(x, y, z);
                                                GL.TexCoord2((double)ix / (double)nx, (double)(iy + 1) / (double)ny);
                                                GL.Vertex3(x, y, z);
                                            }
                                            GL.End();
                                        }
                                    }
                                    private void drawFigure() 
                                    {
                                        GL.Begin(PrimitiveType.Triangles);

                                        GL.Color3(Color.Black);
                                        GL.Vertex3(1.5f * Math.Sin(rotation + 0.2f), 1.5f * Math.Cos(rotation + 0.2f), 1.0f);
                                        GL.Vertex3(0, 0, 1.0f);
                                        GL.Vertex3( 1.5f * Math.Sin(rotation - 0.2f), 1.5f * Math.Cos(rotation - 0.2), 1.0f);
                        
                                        GL.Vertex3(1.5f * Math.Sin(rotation + 3.14f / 2.0f + 0.2f), 1.5f * Math.Cos(rotation + 3.14 / 2.0f + 0.2f), 1.0f);
                                        GL.Vertex3( 0, 0, 1.0f);
                                        GL.Vertex3(1.5f * Math.Sin(rotation + 3.14f / 2.0f - 0.2f), 1.5f * Math.Cos(rotation + 3.14 / 2.0f - 0.2f), 1.0f);

                                        GL.Vertex3(1.5f * Math.Sin(rotation + 2 * 3.14f / 2.0f + 0.2f), 1.5f * Math.Cos(rotation + 2 * 3.14 / 2.0f + 0.2f), 1.0f);
                                        GL.Vertex3(0, 0, 1.0f);
                                        GL.Vertex3(1.5f * Math.Sin(rotation + 2 * 3.14f / 2.0f - 0.2f), 1.5f * Math.Cos(rotation + 2 * 3.14 / 2.0f - 0.2f), 1.0f);

                                        GL.Vertex3(1.5f * Math.Sin(rotation + 3 * 3.14f / 2.0f + 0.2f), 1.5f * Math.Cos(rotation + 3 * 3.14 / 2.0f + 0.2f), 1.0f);
                                        GL.Vertex3(0, 0, 1.0f);
                                        GL.Vertex3(1.5f * Math.Sin(rotation + 3 * 3.14f / 2.0f - 0.2f), 1.5f * Math.Cos(rotation + 3 * 3.14 / 2.0f - 0.2f), 1.0f);

                        
                                        GL.End();
                                        GL.Color3(Color.LightGray);
                                        GL.Enable(EnableCap.Texture2D);
                                        GL.BindTexture(TextureTarget.Texture2D, Textures[0]);
                                        sphere(1, 20, 20);
                                        GL.Disable(EnableCap.Texture2D);

                                    }
                                    public void drawLandScape(float X, float Y, float Z, int cell_size = 1)
                                    {
                                        int grid_size = 64;
                                        int ratioX = map.Width / grid_size;
                                        int ratioY = map.Height / grid_size;
                                        GL.PushMatrix();
                                    
                                        int ix, iy;
                                        double x, y, z;
                                        for(iy = 0; iy < grid_size-1; ++iy)
                                        {
                                            GL.Begin(PrimitiveType.QuadStrip);
                                            for(ix = 0; ix <= grid_size - 1; ++ix)
                                            {
                                                int mapValue = map.GetPixel(ix * ratioX, iy * ratioY).R;
                                                GL.Color3(calculateColor(mapValue));
                                                x = cell_size * (ix - grid_size / 2);
                                                y = cell_size * (iy - grid_size / 2);
                                                z = (double)mapValue / 32 + Z;

                                                GL.Normal3(0, 0, 1);
                                                GL.TexCoord2((float)(ix * ratioX) / map.Width, (float)(iy * ratioY) / map.Height);
                                                GL.Vertex3(x, y ,z);

                                                mapValue = map.GetPixel(ix * ratioX, (iy + 1) * ratioY).R;
                                                GL.Color3(calculateColor(mapValue));                                                
                                                x = cell_size * (ix - grid_size / 2);
                                                y = cell_size * ((iy + 1) - grid_size / 2);
                                                z = (double)mapValue / 32 + Z;
                                                GL.Normal3(0, 0, 1);
                                                GL.TexCoord2((float)(ix * ratioX) / map.Width, (float)((iy + 1) * ratioY) / map.Height);
                                                GL.Vertex3(x, y ,z);
                                            }
                                            GL.End();
                                        }
                                        GL.PopMatrix();  
                                    }

                                    private void glControl1_KeyDown(object sender, KeyEventArgs e)
                                    {
                                        if (!loaded)
                                            return;
                                        float dX = eyeX - directionX;
                                        float dY = eyeY - directionY;
                                        float length = (float)Math.Sqrt(dX * dX + dY * dY);

                                        switch (e.KeyCode) 
                                        {
                                            case Keys.W:
                                                eyeX -= dX * 0.01f * length;
                                                eyeY -= dY * 0.01f * length;
                                                directionX -= dX * 0.01f * length;
                                                directionY -= dY * 0.01f * length;
                                                break;
                                            case Keys.S:
                                                eyeX += dX * 0.01f * length;
                                                eyeY += dY * 0.01f * length;
                                                directionX += dX * 0.01f * length;
                                                directionY += dY * 0.01f * length;
                                                break;
                                            case Keys.A:
                                                eyeX += dY * 0.01f;
                                                eyeY -= dX * 0.01f;
                                                break;
                                            case Keys.D:
                                                eyeX -= dY * 0.01f;
                                                eyeY += dX * 0.01f;
                                                break;
                                            case Keys.Space:
                                                eyeZ += 0.1f;
                                                directionZ += 0.1f;
                                                break;
                                            case Keys.Z:
                                                eyeZ -= 0.1f;
                                                directionZ -= 0.1f;
                                                break;
                                        }
                                        glControl1.Invalidate();
                                    }
                                    int loadTexture(String filename) 
                                    {
                                        try
                                        {
                                            Bitmap image = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\" + filename);

                                            int texID = GL.GenTexture();

                                            GL.BindTexture(TextureTarget.Texture2D, texID);
                                            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                                            image.UnlockBits(data);

                                            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                                            return texID;
                                        }
                                        catch(FileNotFoundException) 
                                        {
                                            return -1;
                                        }
                                    }
                                    private void setupLighting()
                                    {
                                        float[] light_ambiant = { 0.3f, 0.3f, 0.3f, 1.0f };
                                        float[] light_diffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
                                        float[] light_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
                                        float[] spotdirection = { 0.0f, 0.0f, -1.0f };
                                        GL.Light(LightName.Light0, LightParameter.Ambient, light_ambiant);
                                        GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
                                        GL.Light(LightName.Light0, LightParameter.Specular, light_specular);
                                        
                                        GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, 1.8f);
                                        GL.Light(LightName.Light0, LightParameter.SpotCutoff, 45.0f);
                                        GL.Light(LightName.Light0, LightParameter.SpotDirection, spotdirection);
                                        GL.Light(LightName.Light0, LightParameter.SpotExponent, 1.0f);

                                        GL.LightModel(LightModelParameter.LightModelLocalViewer, 1.0f);
                                        GL.LightModel(LightModelParameter.LightModelTwoSide, 1.0f);
                                        GL.Enable(EnableCap.Light0);
                                        GL.Enable(EnableCap.Lighting);
                                        GL.Enable(EnableCap.DepthTest);
                                        GL.Enable(EnableCap.ColorMaterial);   
                                        GL.ShadeModel(ShadingModel.Smooth);
                                    }
                                    void drawCylinder(double radius, float width, Vector3 center, Vector3 normal, int nx, int ny) 
                                    {
                                        int ix, iy;
                                        double x, y, z;
                                        GL.Translate(center);
                                        if (normal != Vector3.UnitZ)
                                        {
                                            normal.Normalize();
                                            GL.Rotate(57.2957795* Math.Acos(normal.Z), new Vector3d(normal.Y , -normal.X, 0));
                                        }
                                        for (iy = 0; iy < ny; ++iy) 
                                        {
                                            GL.Begin(BeginMode.QuadStrip);
                                            for (ix = 0; ix <= nx; ++ix)
                                            {
                                                x = radius * Math.Cos(2* ix * Math.PI / nx);
                                                y = radius * Math.Sin(2 * ix * Math.PI / nx);
                                                z = iy / ny * width;
                                                GL.Normal3(x, y, 0);
                                                GL.TexCoord2((double)ix / (double)nx, (double)iy / (double)ny);
                                                GL.Vertex3(x, y, z);

                                                x = radius * Math.Cos(2 * ix * Math.PI / nx);
                                                y = radius * Math.Sin(2 * ix * Math.PI / nx);
                                                z = (iy+1) / ny * width;
                                                GL.Normal3(x, y, 0);
                                                GL.TexCoord2((double)ix / (double)nx, (double)iy / (double)ny);
                                                GL.Vertex3(x, y, z);
                                            }
                                            GL.End();
                                        }
                                        for (int i = 0; i < 2; i++) 
                                        {
                                            GL.Begin(BeginMode.TriangleFan);
                                            GL.TexCoord2(0.5f, 0.5f);
                                            GL.Vertex3(0, 0, i * width);
                                            for (ix = 0; ix <= nx; ++ix) 
                                            {
                                                x = radius * Math.Cos(2 * ix * Math.PI / nx);
                                                y = radius * Math.Sin(2 * ix * Math.PI / nx);
                                                double s = (x + radius) / (2 * radius);
                                                double t = (y + radius) / (2 * radius);
                                                GL.TexCoord2(t, s);
                                                GL.Vertex3(x, y, i * width);
                                            }
                                            GL.End();
                                        }
                                        if(normal !=Vector3.UnitZ)
                                            GL.Rotate(-57.2957795 * Math.Acos(normal.Z), new Vector3d(normal.Y, - normal.X, 0));
                                        GL.Translate(-center);
                                    }
                                }
                                public class Coin
                                {
                                    public double radius;
                                    public float width;
                                    public Vector3 center;
                                    public Vector3 normal;
                                    public bool touched;
                                    public Coin(double newRadius, float newWidth, Vector3 newCenter, Vector3 newNormal) 
                                    {
                                        this.radius = newRadius;
                                        this.width = newWidth;
                                        this.center = newCenter;
                                        this.normal = newNormal;
                                        this.touched = false;
                                    }
                                };

                            }