﻿using System;
using Tao.FreeGlut;
using OpenGL;

namespace CarParticle
{
    class Program
    {
        private static CarModel carmodel;

        private static int width = 1280, height = 720;
        private static ShaderProgram program;

        private static System.Diagnostics.Stopwatch watch;
        private static float xangle, yangle;
        private static bool autoRotate, lighting = true, fullscreen = false, alpha = true;
        private static bool left, right, up, down;

        private static float maximumAmbient = 10f;
        private static float minimumAmbient = 0f;
        private static float maximumDiffuse = 10f;
        private static float minimumDiffuse = 0f;

        private static float ambient = 0.3f;
        private static float maxdiffuse = 1f;

        //Rain
        private static List<Particle> particles = new List<Particle>();
        private static int particleCount = 2000;
        private static Vector3[] particlePositions = new Vector3[particleCount];
        private static Random generator = new Random();


        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("3D Car Particle");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);
            Glut.glutMouseWheelFunc(MouseWheel);

            Glut.glutCloseFunc(OnClose);
            Glut.glutReshapeFunc(OnReshape);

            Gl.Disable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.Enable(EnableCap.ProgramPointSize);
            Gl.Enable(EnableCap.Multisample);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            program = new ShaderProgram(VertexShader, FragmentShader);

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 6), new Vector3(1.2f,0.2f,0f), new Vector3(0, 1, 0)));

            program["light_direction"].SetValue(new Vector3(0, 0, 1));
            program["enable_lighting"].SetValue(lighting);

            carmodel = new CarModel();
            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        }

        private static void OnClose()
        {
            CarModel.cube.Dispose();
            CarModel.cubeNormals.Dispose();
            CarModel.cubeUV.Dispose();
            CarModel.cubeQuads.Dispose();
            CarModel.bodyTexture.Dispose();
            CarModel.kacaTexture.Dispose();
            CarModel.rodaTexture.Dispose();
            program.DisposeChildren = true;
            program.Dispose();



        }

        private static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            // perform rotation of the cube depending on the keyboard state
            if (autoRotate)
            {
                xangle += deltaTime / 2;
                yangle += deltaTime;
            }
            if (right) yangle += deltaTime;
            if (left) yangle -= deltaTime;
            if (up) xangle -= deltaTime;
            if (down) xangle += deltaTime;

            // set up the viewport and clear the previous depth and color buffers
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.ClearColor(30f/255,144f/255,255f/255,255f/255);
            // make sure the shader program and texture are being used
            Gl.UseProgram(program);
            Gl.BindTexture(CarModel.bodyTexture);

            // set up the model matrix and draw the cube
            program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle)  * Matrix4.CreateRotationX(xangle));
            program["enable_lighting"].SetValue(lighting);

            Gl.BindBufferToShaderAttribute(CarModel.cube, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(CarModel.cubeNormals, program, "vertexNormal");
            Gl.BindBufferToShaderAttribute(CarModel.cubeUV, program, "vertexUV");
            Gl.BindBuffer(CarModel.cubeQuads);
            
            Gl.DrawElements(BeginMode.Quads, CarModel.cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            //Draw window
            Gl.BindTexture(CarModel.kacaTexture);
            
            program["max_diffuse"].SetValue(maxdiffuse);
            program["ambient"].SetValue(ambient);

            Gl.BindBufferToShaderAttribute(CarModel.window, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(CarModel.windowNormals, program, "vertexNormal");
            Gl.BindBufferToShaderAttribute(CarModel.windowUV, program, "vertexUV");
            Gl.BindBuffer(CarModel.windowQuads);

            Gl.DrawElements(BeginMode.Quads, CarModel.windowQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //drawwheel
            Gl.BindTexture(CarModel.rodaTexture);

            Gl.BindBufferToShaderAttribute(CarModel.wheel1, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(CarModel.wheel1Normals, program, "vertexNormal");
            Gl.BindBufferToShaderAttribute(CarModel.wheel1UV, program, "vertexUV");
            Gl.BindBuffer(CarModel.wheel1Quads);
            Gl.DrawElements(BeginMode.TriangleStrip, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.wheel2, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleStrip, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.wheel2Normals, program, "vertexNormal");

            Gl.BindBufferToShaderAttribute(CarModel.wheel3, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleStrip, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.wheel4, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleStrip, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //Gl.BindTexture(velgTexture);
            Gl.BindBufferToShaderAttribute(CarModel.wheel1Normals, program, "vertexNormal");

            Gl.BindBufferToShaderAttribute(CarModel.velg1, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleFan, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.velg2, program, "vertexPosition");

            Gl.DrawElements(BeginMode.TriangleFan, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.wheel2Normals, program, "vertexNormal");


            Gl.BindBufferToShaderAttribute(CarModel.velg3, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleFan, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(CarModel.velg4, program, "vertexPosition");
            Gl.DrawElements(BeginMode.TriangleFan, CarModel.wheel1Quads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Glut.glutSwapBuffers();
        }

        private static void OnReshape(int width, int height)
        {
            Program.width = width;
            Program.height = height;

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
        }

        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 'w') up = true;
            else if (key == 's') down = true;
            else if (key == 'd') right = true;
            else if (key == 'a') left = true;
            else if (key == 'p')
            {
                if (maxdiffuse <= maximumDiffuse)
                {
                    maxdiffuse = maxdiffuse + 0.1f;
                }
            }
            else if (key == 'o')
            {
                if (maxdiffuse >= minimumDiffuse)
                {
                    maxdiffuse = maxdiffuse - 0.1f;
                }
            }
            else if (key == ']')
            {
                if (ambient <= maximumAmbient)
                {
                    ambient = ambient + 0.05f;
                }
            }
            else if (key == '[')
            {
                if (ambient >= minimumAmbient)
                {
                    ambient = ambient - 0.05f;
                }
            }
            else if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == 'w') up = false;
            else if (key == 's') down = false;
            else if (key == 'd') right = false;
            else if (key == 'a') left = false;
            else if (key == ' ') autoRotate = !autoRotate;
            else if (key == 'l') lighting = !lighting;
            else if (key == 'z')
            {
                fullscreen = !fullscreen;
                if (fullscreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);
                }
            }
            else if (key == 'b')
            {
                alpha = !alpha;
                if (alpha)
                {
                    Gl.Enable(EnableCap.Blend);
                    Gl.Disable(EnableCap.DepthTest);
                }
                else
                {
                    Gl.Disable(EnableCap.Blend);
                    Gl.Enable(EnableCap.DepthTest);
                }
            }
        }

        private static void MouseWheel(int wheel, int direction, int x, int y)
        {
            if (direction > 0)
            {
                if (maxdiffuse <= maximumDiffuse)
                {
                    maxdiffuse = maxdiffuse + 0.1f;
                }
            } else
            {
                if (maxdiffuse >= minimumDiffuse)
                {
                    maxdiffuse = maxdiffuse - 0.1f;
                }
            }
        }

        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;

out vec3 normal;
out vec2 uv;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
    uv = vertexUV;

    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130

uniform sampler2D texture;
uniform vec3 light_direction;
uniform bool enable_lighting;
uniform float ambient;
uniform float max_diffuse;

in vec3 normal;
in vec2 uv;

out vec4 fragment;

void main(void)
{
    float diffuse = max(dot(normal, light_direction), 0) * max_diffuse;
    float lighting = (enable_lighting ? max(diffuse, ambient) : 1);

    // add in some blending for tutorial 8 by setting the alpha to 0.5
    fragment = vec4(lighting * texture2D(texture, uv).xyz, 0.5);
}
";
    }
}
