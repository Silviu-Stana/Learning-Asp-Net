using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl
{
    public class Pyramid
    {
        private int _vao, _vbo, _ebo, _edgeEbo;
        private int _shaderProgram;

        // Camera matrices
        private Matrix4 _view;
        private Matrix4 _projection;

        private float _aspectRatio;

        public float RotationX { get; set; } = 0f;
        public float RotationY { get; set; } = 0f;

        public void Load(int windowWidth, int windowHeight)
        {
            float[] vertices =
            {
            // Base square (y = -0.5)
            -0.5f, -0.5f, -0.5f, // 0
             0.5f, -0.5f, -0.5f, // 1
             0.5f, -0.5f,  0.5f, // 2
            -0.5f, -0.5f,  0.5f, // 3
            // Apex
             0.0f,  0.5f,  0.0f  // 4
        };

            uint[] indices =
            {
            // 4 triangular sides
            0, 1, 4,
            1, 2, 4,
            2, 3, 4,
            3, 0, 4,
            // Base (two triangles)
            0, 1, 2,
            2, 3, 0
        };

            uint[] edgeIndices =
            {
            // base edges
            0,1, 1,2, 2,3, 3,0,
            // side edges
            0,4, 1,4, 2,4, 3,4
        };

            //Smooth lines: (fix aliasing)
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Multisample);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();
            _edgeEbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            // Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Triangle EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Edge EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _edgeEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, edgeIndices.Length * sizeof(uint), edgeIndices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);

            // Camera setup
            _view = Matrix4.LookAt(new Vector3(2, 2, 3), Vector3.Zero, Vector3.UnitY);
            Resize(windowWidth, windowHeight);
        }

        public void Resize(int width, int height)
        {
            if (height == 0) height = 1;
            _aspectRatio = width / (float)height;

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

            // Rotate model
            Matrix4 model =Matrix4.CreateRotationX(RotationX) * Matrix4.CreateRotationY(RotationY);

            // Upload matrices
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uModel"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uView"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref _projection);

            int colorLoc = GL.GetUniformLocation(_shaderProgram, "uColor");

            // Draw filled purple pyramid
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.Uniform4(colorLoc, 0.866f, 0.204f, 0.878f, 1.0f); // purple
            GL.DrawElements(PrimitiveType.Triangles, 18, DrawElementsType.UnsignedInt, 0);

            // Draw white outline
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _edgeEbo);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(20f);
            GL.Uniform4(colorLoc, 1f, 1f, 1f, 1f); // white
            GL.DrawElements(PrimitiveType.Lines, 16, DrawElementsType.UnsignedInt, 0);

            // Reset polygon mode
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        public void Unload()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteBuffer(_edgeEbo);
            GL.DeleteProgram(_shaderProgram);
        }

        // ------------------------------------------------------------
        // SHADERS
        // ------------------------------------------------------------
        private const string VertexShaderSource = @"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;
        void main()
        {
        gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
        }";

        private const string FragmentShaderSource = @"
        #version 330 core
        uniform vec4 uColor;
        out vec4 FragColor;
        void main()
        {
        FragColor = uColor;
        }
        ";

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