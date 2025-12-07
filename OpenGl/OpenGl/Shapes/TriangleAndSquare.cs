using OpenTK.Graphics.OpenGL4;

namespace OpenGl.Shapes
{
    public class TriangleAndSquare
    {
        private int _triangleVao;
        private int _triangleVbo;
        private int _triangleShader;

        private int _squareVao;
        private int _squareVbo;
        private int _squareShader;

        public void Load()
        {
            LoadTriangle();
            LoadSquare();
        }

        public void Render()
        {
            // Use triangle shader
            GL.UseProgram(_triangleShader);
            GL.BindVertexArray(_triangleVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            // Use square shader
            GL.UseProgram(_squareShader);
            GL.BindVertexArray(_squareVao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public void Unload()
        {
            GL.DeleteVertexArray(_triangleVao);
            GL.DeleteBuffer(_triangleVbo);
            GL.DeleteProgram(_triangleShader);

            GL.DeleteVertexArray(_squareVao);
            GL.DeleteBuffer(_squareVbo);
            GL.DeleteProgram(_squareShader);
        }

        // ------------------------------------------------------------
        // TRIANGLE (With Vertex Shader)
        // ------------------------------------------------------------
        private void LoadTriangle()
        {
            float[] vertices =
{
    // Position      // Color (RGB)
     0.0f,  0.5f,     1.0f, 0.0f, 0.0f,   // Top (Red)
    -0.5f, -0.5f,     0.0f, 1.0f, 0.0f,   // Bottom-left (Green)
     0.5f, -0.5f,     0.0f, 0.0f, 1.0f    // Bottom-right (Blue)
};

            _triangleVao = GL.GenVertexArray();
            _triangleVbo = GL.GenBuffer();

            GL.BindVertexArray(_triangleVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _triangleVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute (location = 0)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute (location = 1)
            GL.VertexAttribPointer(
                1, 3, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);


            _triangleShader = CreateShaderProgram(
                vertexSource: TriangleVertexShader,
                fragmentSource: TriangleFragmentShader);
        }

        // ------------------------------------------------------------
        // SQUARE
        // ------------------------------------------------------------
        private void LoadSquare()
        {
            float[] vertices =
            {
                -0.8f,  0.8f,
                -0.2f,  0.8f,
                -0.8f,  0.2f,
                -0.2f,  0.2f
            };

            _squareVao = GL.GenVertexArray();
            _squareVbo = GL.GenBuffer();

            GL.BindVertexArray(_squareVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _squareVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _squareShader = CreateShaderProgram(
                vertexSource: SquareVertexShader,
                fragmentSource: SquareFragmentShader);
        }

        // ------------------------------------------------------------
        // SHADER HELPERS
        // ------------------------------------------------------------
        private int CreateShaderProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        // ------------------------------------------------------------
        // SHADER SOURCES
        // ------------------------------------------------------------
        private const string TriangleVertexShader = @"
#version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec3 aColor;

out vec3 vColor;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}
";

        private const string TriangleFragmentShader = @"
#version 330 core

in vec3 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, 1.0);
}
";

        private const string SquareVertexShader = @"
#version 330 core
layout(location = 0) in vec2 aPosition;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
}
";

        private const string SquareFragmentShader = @"
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(0.2, 0.8, 0.2, 1.0);  // Green-ish
}
";
    }
}
