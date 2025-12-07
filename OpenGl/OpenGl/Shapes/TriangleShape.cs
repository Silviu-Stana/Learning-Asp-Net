using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl.Shapes
{
    public class TriangleShape
    {
        private int _vao;
        private int _vbo;
        private int _shader;

        public void Load()
        {
            float[] vertices =
            {
                // Position      // Color
                 0.0f,  0.5f,    1.0f, 0.0f, 0.0f, // Top (Red)
                -0.5f, -0.5f,    0.0f, 1.0f, 0.0f, // Bottom-left (Green)
                 0.5f, -0.5f,    0.0f, 0.0f, 1.0f  // Bottom-right (Blue)
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader = CreateShaderProgram(TriangleVertexShader, TriangleFragmentShader);
        }

        public void Render()
        {
            float time = (float)GLFW.GetTime();
            Matrix4 rotation = Matrix4.CreateRotationY(time);

            GL.UseProgram(_shader);
            int loc = GL.GetUniformLocation(_shader, "uTransform");
            GL.UniformMatrix4(loc, false, ref rotation);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
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

        private const string TriangleVertexShader = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec3 aColor;

uniform mat4 uTransform;
out vec3 vColor;

void main()
{
    gl_Position = uTransform * vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}";

        private const string TriangleFragmentShader = @"
#version 330 core
in vec3 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, 1.0);
}";
    }
}
