using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.Processing;

namespace OpenGl
{
    public class CubeShadow : IDisposable
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

        public CubeShadow()
        {
            LoadShaders();
            LoadBuffers();
            LoadTexture("Assets/Textures/tile.jpg");
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
out vec3 vNormal;
out vec3 vFragPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vTex = aTexCoord;

    // world-space fragment position
    vFragPos = vec3(model * vec4(aPosition, 1.0));

    // world-space normal
    vNormal = mat3(transpose(inverse(model))) * aNormal;

    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}
";

//Bright front face
//Soft shadow on sides
//Back face dark
//Fully textured
            const string fragmentShaderSrc =
@"
#version 330 core
out vec4 FragColor;

in vec2 vTex;
in vec3 vNormal;
in vec3 vFragPos;

uniform sampler2D tex;

// simple light
uniform vec3 lightPos = vec3(0.0, 0.0, 2.0);
uniform vec3 lightColor = vec3(1.0, 1.0, 1.0);

void main()
{
    vec3 color = texture(tex, vTex).rgb;

    // normalize normal
    vec3 N = normalize(vNormal);

    // light direction
    vec3 L = normalize(lightPos - vFragPos);

    // diffuse shading
    float diff = max(dot(N, L), 0.0);

    // ambient (softens dark faces)
    float ambient = 0.25;

    vec3 result = color * (diff + ambient);

    FragColor = vec4(result, 1.0);
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

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

            GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,img.Width,img.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,
                pixelData);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
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
