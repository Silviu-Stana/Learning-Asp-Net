using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.Processing;

namespace OpenGl
{
    public class TexturedCube : IDisposable
    {
        private readonly float[] _vertices =
        {
            //   POSITION           NORMALS      UV
            // Front face
            -0.5f, -0.5f,  0.5f,    0, 0, 1,     0, 0,
             0.5f, -0.5f,  0.5f,    0, 0, 1,     1, 0,
             0.5f,  0.5f,  0.5f,    0, 0, 1,     1, 1,
            -0.5f,  0.5f,  0.5f,    0, 0, 1,     0, 1,

            // Back face
            -0.5f, -0.5f, -0.5f,    0, 0,-1,     1, 0,
             0.5f, -0.5f, -0.5f,    0, 0,-1,     0, 0,
             0.5f,  0.5f, -0.5f,    0, 0,-1,     0, 1,
            -0.5f,  0.5f, -0.5f,    0, 0,-1,     1, 1,

            // Left face
            -0.5f, -0.5f, -0.5f,   -1, 0, 0,     0, 0,
            -0.5f, -0.5f,  0.5f,   -1, 0, 0,     1, 0,
            -0.5f,  0.5f,  0.5f,   -1, 0, 0,     1, 1,
            -0.5f,  0.5f, -0.5f,   -1, 0, 0,     0, 1,

            // Right face
             0.5f, -0.5f, -0.5f,    1, 0, 0,     1, 0,
             0.5f, -0.5f,  0.5f,    1, 0, 0,     0, 0,
             0.5f,  0.5f,  0.5f,    1, 0, 0,     0, 1,
             0.5f,  0.5f, -0.5f,    1, 0, 0,     1, 1,

            // Top face
            -0.5f,  0.5f,  0.5f,    0, 1, 0,     0, 0,
             0.5f,  0.5f,  0.5f,    0, 1, 0,     1, 0,
             0.5f,  0.5f, -0.5f,    0, 1, 0,     1, 1,
            -0.5f,  0.5f, -0.5f,    0, 1, 0,     0, 1,

            // Bottom face
            -0.5f, -0.5f,  0.5f,    0,-1, 0,     0, 1,
             0.5f, -0.5f,  0.5f,    0,-1, 0,     1, 1,
             0.5f, -0.5f, -0.5f,    0,-1, 0,     1, 0,
            -0.5f, -0.5f, -0.5f,    0,-1, 0,     0, 0
        };

        private readonly uint[] _indices =
        {
            0, 1, 2,   2, 3, 0,          // front
            4, 5, 6,   6, 7, 4,          // back
            8, 9,10,   10,11,8,          // left
            12,13,14,  14,15,12,         // right
            16,17,18,  18,19,16,         // top
            20,21,22,  22,23,20          // bottom
        };

        private int _vao, _vbo, _ebo;
        private int _shader;
        private int _texture;

        private float _rotation;

        public TexturedCube()
        {
            LoadShaders();
            LoadBuffers();
            LoadTexture("Assets/Textures/crate.jpg");
        }

        private void LoadShaders()
        {
            const string vertexShaderSrc =
@"
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

out vec2 vTex;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vTex = aTexCoord;
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}
";

            const string fragmentShaderSrc =
@"
#version 330 core
out vec4 FragColor;

in vec2 vTex;

uniform sampler2D tex;

void main()
{
    FragColor = texture(tex, vTex);
}
";

            int vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexShaderSrc);
            GL.CompileShader(vertex);

            int fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentShaderSrc);
            GL.CompileShader(fragment);

            _shader = GL.CreateProgram();
            GL.AttachShader(_shader, vertex);
            GL.AttachShader(_shader, fragment);
            GL.LinkProgram(_shader);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        private void LoadBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            //Upload vertex data to VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //Upload index data to EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            //Each vertex = 8 floats (3 pos + 3 normal + 2 UV).
            int stride = 8 * sizeof(float);

            //Attribute 0 — Position, has 3 floats
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0); //starts at offset 0

            //Attribute 1 — Normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float)); //starts at offset 3

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float)); //offset=6

            //prevents accidental changes.
            GL.BindVertexArray(0);
        }

        private void LoadTexture(string path)
        {
            using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(path);


            // Flip vertically because OpenGL expects origin at bottom-left
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            // Get raw pixel data
            byte[] pixelData = new byte[4 * img.Width * img.Height];
            img.CopyPixelDataTo(pixelData);

            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte,
                pixelData);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //Highest quality filter. Linear interpolation between mip levels
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            //Use linear filtering when the texture gets bigger → smoother, less blocky.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //how textures behave when UV coordinates go outside the 0–1 range.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); //repeat in X dir (S)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); //repeat in Y dir (T)
        }

        public void Render(float delta, Matrix4 view, Matrix4 projection)
        {
            _rotation += delta;

            Matrix4 model =
                Matrix4.CreateRotationY(_rotation) *
                Matrix4.CreateRotationX(_rotation * 0.5f);

            GL.UseProgram(_shader);

            GL.UniformMatrix4(GL.GetUniformLocation(_shader, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shader, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shader, "projection"), false, ref projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteProgram(_shader);
        }
    }
}
