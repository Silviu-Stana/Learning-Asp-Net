
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl
{
    public class Cube
    {
        private int _vao, _vbo, _ebo;
        private int _shaderProgram;

        // Camera matrices
        private Matrix4 _view;
        private Matrix4 _projection;

        // Animation time and current rotation
        private float _time = 0.0f;
        private float _currentRotationX = 0.0f;
        private float _currentRotationY = 0.0f;

        private float _aspectRatio;

        public void Load(int windowWidth, int windowHeight)
        {
            float[] vertices =
            {
                // Position xyz        // Color rgb
                -0.5f, -0.5f, -0.5f,     1f, 0f, 0f,
                 0.5f, -0.5f, -0.5f,     0f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,     0f, 0f, 1f,
                -0.5f,  0.5f, -0.5f,     1f, 1f, 0f,

                -0.5f, -0.5f,  0.5f,     1f, 0f, 1f,
                 0.5f, -0.5f,  0.5f,     0f, 1f, 1f,
                 0.5f,  0.5f,  0.5f,     1f, 1f, 1f,
                -0.5f,  0.5f,  0.5f,     0f, 0f, 0f,
            };

            uint[] indices =
            {
                // Back face
                0, 1, 2, 2, 3, 0,
                // Front face
                4, 5, 6, 6, 7, 4,
                // Left
                0, 3, 7, 7, 4, 0,
                // Right
                1, 5, 6, 6, 2, 1,
                // Bottom
                0, 1, 5, 5, 4, 0,
                // Top
                3, 2, 6, 6, 7, 3
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position (location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color (location = 1)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);

            // Simple camera setup
            _view = Matrix4.LookAt(new Vector3(2, 2, 3), Vector3.Zero, Vector3.UnitY);
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                windowWidth / (float)windowHeight,
                0.1f,
                100f
            );

            // Initial camera setup (View matrix is static unless you move the camera)
            _view = Matrix4.LookAt(new Vector3(2, 2, 3), Vector3.Zero, Vector3.UnitY);

            // Set initial projection
            Resize(windowWidth, windowHeight);
        }

        //Handles animation Every frame.
        public void Update(float timeDelta)
        {
            // Accumulate time for animation
            _time += timeDelta;

            // Calculate new rotation angles
            // Use timeDelta (dTime) for frame-rate independent movement
            _currentRotationY += 1.0f * timeDelta; // Rotate 1.0 radian per second around Y
            _currentRotationX += 0.5f * timeDelta; // Rotate 0.5 radians per second around X
        }


        //Recalculate Projection Matrix
        public void Resize(int width, int height)
        {
            if (height == 0) height = 1; // Avoid division by zero
            _aspectRatio = width / (float)height;

            // Recalculate the Projection matrix with the new aspect ratio
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                _aspectRatio,
                0.1f,
                100f
            );
        }

        public void Render()
        {
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            float t = (float)GLFW.GetTime();

            // Rotate model over time
            Matrix4 model =
                Matrix4.CreateRotationY(t) *
                Matrix4.CreateRotationX(t * 0.5f);

            // Upload matrices
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uModel"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uView"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref _projection);

            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        }

        public void Unload()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteProgram(_shaderProgram);
        }

        // ------------------------------------------------------------
        // SHADERS
        // ------------------------------------------------------------

        private const string VertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;

out vec3 vColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    vColor = aColor;
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
}
";

        private const string FragmentShaderSource = @"
#version 330 core

in vec3 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, 1.0);
}
";

        // ------------------------------------------------------------
        // Shader helper
        // ------------------------------------------------------------
        private int CreateShaderProgram(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);

            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }
    }
}
