using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl.Shapes
{
    public class SquareShape
    {
        private int _vao;
        private int _vbo;
        private int _shader;

        public void Load()
        {
            float[] vertices =
            {
                -0.8f,  0.8f,
                -0.2f,  0.8f,
                -0.8f,  0.2f,
                -0.2f,  0.2f
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader = CreateShaderProgram(SquareVertexShader, SquareFragmentShader);
        }

        public void Render()
        {
            float time = (float)GLFW.GetTime();

            // Rotate around X-axis
            Matrix4 rotation =
                Matrix4.CreateTranslation(0.5f, -0.5f, 0f) *
                Matrix4.CreateRotationX(time) *
                Matrix4.CreateTranslation(-0.5f, 0.5f, 0f);

            GL.UseProgram(_shader);
            int loc = GL.GetUniformLocation(_shader, "uTransform");
            GL.UniformMatrix4(loc, false, ref rotation);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public void Unload()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteProgram(_shader);
        }

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

        private const string SquareVertexShader = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
uniform mat4 uTransform;
void main()
{
    gl_Position = uTransform * vec4(aPosition, 0.0, 1.0);
}";

        private const string SquareFragmentShader = @"
#version 330 core
out vec4 FragColor;
void main()
{
    FragColor = vec4(0.2, 0.8, 0.2, 1.0);
}";
    }
}
