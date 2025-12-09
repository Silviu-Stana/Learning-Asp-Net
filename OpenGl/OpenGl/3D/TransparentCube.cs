using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.Processing;

namespace OpenGl
{
    public class TransparentCube
    {
        private int _vao, _vbo, _ebo;
        private int _shaderProgram;

        private Matrix4 _view;
        private Matrix4 _projection;

        private int _texture=0;
        private float _aspectRatio;

        public float Alpha { get; set; } = 0.35f; // <— transparency (0=inv, 1=solid)

        public void Load(int windowWidth, int windowHeight)
        {
            float[] vertices =
            {
                // --- Front face ---
                -0.5f, -0.5f,  0.5f,   0f, 0f,
                 0.5f, -0.5f,  0.5f,   1f, 0f,
                 0.5f,  0.5f,  0.5f,   1f, 1f,
                -0.5f,  0.5f,  0.5f,   0f, 1f,

                // --- Back face ---
                 0.5f, -0.5f, -0.5f,   0f, 0f,
                -0.5f, -0.5f, -0.5f,   1f, 0f,
                -0.5f,  0.5f, -0.5f,   1f, 1f,
                 0.5f,  0.5f, -0.5f,   0f, 1f,

                // --- Left face ---
                -0.5f, -0.5f, -0.5f,   0f, 0f,
                -0.5f, -0.5f,  0.5f,   1f, 0f,
                -0.5f,  0.5f,  0.5f,   1f, 1f,
                -0.5f,  0.5f, -0.5f,   0f, 1f,

                // --- Right face ---
                 0.5f, -0.5f,  0.5f,   0f, 0f,
                 0.5f, -0.5f, -0.5f,   1f, 0f,
                 0.5f,  0.5f, -0.5f,   1f, 1f,
                 0.5f,  0.5f,  0.5f,   0f, 1f,

                // --- Top face ---
                -0.5f,  0.5f,  0.5f,   0f, 0f,
                 0.5f,  0.5f,  0.5f,   1f, 0f,
                 0.5f,  0.5f, -0.5f,   1f, 1f,
                -0.5f,  0.5f, -0.5f,   0f, 1f,

                // --- Bottom face ---
                -0.5f, -0.5f, -0.5f,   0f, 0f,
                 0.5f, -0.5f, -0.5f,   1f, 0f,
                 0.5f, -0.5f,  0.5f,   1f, 1f,
                -0.5f, -0.5f,  0.5f,   0f, 1f,
            };

            uint[] indices =
            {
                0,1,2, 2,3,0,
                4,5,6, 6,7,4,
                8,9,10, 10,11,8,
                12,13,14, 14,15,12,
                16,17,18, 18,19,16,
                20,21,22, 22,23,20
            };

            // Enable blending for transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Important for transparency: draw back faces first
            GL.Disable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Front);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindTexture(TextureTarget.Texture2D, _texture);

            // Texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Load file
            using (var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>("Assets/Textures/glass.jpg"))
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                var pixels = new byte[4 * image.Width * image.Height];
                image.CopyPixelDataTo(pixels);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Position (location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // UV (location = 1)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);

            _view = Matrix4.LookAt(new Vector3(2, 2, 3), Vector3.Zero, Vector3.UnitY);

            Resize(windowWidth, windowHeight);
        }

        public void Update(float dt) { }

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

            Matrix4 model =
                Matrix4.CreateRotationY(t * 0.8f) *
                Matrix4.CreateRotationX(t * 0.4f);

            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uModel"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uView"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref _projection);

            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "uAlpha"), Alpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "uTexture"), 0);

            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        }

        public void Unload()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteProgram(_shaderProgram);
        }

        // ---------------------------------------------------------------------
        // SHADERS
        // ---------------------------------------------------------------------
        private const string VertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aUV;

out vec2 vUV;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    vUV = aUV;
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
}
";

        private const string FragmentShaderSource = @"
#version 330 core

in vec2 vUV;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform float uAlpha;

void main()
{
    vec4 texColor = texture(uTexture, vUV);
    FragColor = vec4(texColor.rgb, texColor.a * uAlpha);
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
