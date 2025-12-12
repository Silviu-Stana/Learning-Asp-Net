using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl
{
    public class Star
    {
        // GPU object handles
        private int _vao, _vbo, _ebo;
        private int _shaderProgram;
        private int _indexCount;

        // Camera matrices
        private Matrix4 _view;
        private Matrix4 _projection;

        // Time (used for animations)
        private float _time;
        private float _aspectRatio;

        public void Load(int width, int height)
        {
            // ------------------------------------
            // Build the 3D geometry of the star
            // ------------------------------------
            List<float> vertices = new();
            List<uint> indices = new();

            // Radius for outer + inner star tips
            float outerR = 0.5f;
            float innerR = 0.22f;

            // Depth for the 3D extrusion
            float frontZ = 0.1f;
            float backZ = -0.1f;

            // ------------------------------------
            // Create the 10-point 2D star outline
            // (outer, inner, outer, inner, ...)
            // ------------------------------------
            Vector2[] basePoints = new Vector2[10];

            for (int i = 0; i < 10; i++)
            {
                // Angle around circle
                float angle = MathF.PI * 2f * (i / 10f);

                // Alternate radius (outer or inner)
                float r = (i % 2 == 0) ? outerR : innerR;

                // Convert to xy position
                basePoints[i] = new Vector2(
                    MathF.Cos(angle) * r,
                    MathF.Sin(angle) * r
                );
            }

            // ------------------------------------
            // Create FRONT face vertices (indices 0..9)
            // ------------------------------------
            for (int i = 0; i < 10; i++)
            {
                // Position
                vertices.Add(basePoints[i].X);
                vertices.Add(basePoints[i].Y);
                vertices.Add(frontZ);

                // Color (gold)
                vertices.Add(1f);
                vertices.Add(1f);
                vertices.Add(0f);
            }

            // Add center vertex for triangle fan (index 10)
            vertices.Add(0f); vertices.Add(0f); vertices.Add(frontZ);

            // White center (gives nice gold→white gradient)
            vertices.Add(1f); vertices.Add(1f); vertices.Add(1f);

            // ------------------------------------
            // Create BACK face vertices (indices 11..20)
            // ------------------------------------
            for (int i = 0; i < 10; i++)
            {
                // Position
                vertices.Add(basePoints[i].X);
                vertices.Add(basePoints[i].Y);
                vertices.Add(backZ);

                // Color (currently red-ish)
                vertices.Add(1f);
                vertices.Add(0f);
                vertices.Add(0f);
            }

            // Add back center vertex (index 21)
            vertices.Add(0f); vertices.Add(0f); vertices.Add(backZ);
            vertices.Add(1f); vertices.Add(1f); vertices.Add(1f);

            // ------------------------------------
            // FRONT FACE INDICES (triangle fan)
            // ------------------------------------
            uint frontCenter = 10;

            for (uint i = 0; i < 10; i++)
            {
                uint next = (i + 1) % 10;

                indices.Add(frontCenter);
                indices.Add(i);
                indices.Add(next);
            }

            // ------------------------------------
            // BACK FACE INDICES (triangle fan)
            // Reversed winding so the back points outward
            // ------------------------------------
            uint backOffset = 11;
            uint backCenter = 21;

            for (uint i = 0; i < 10; i++)
            {
                uint cur = backOffset + i;
                uint next = backOffset + ((i + 1) % 10);

                indices.Add(backCenter);
                indices.Add(next); // reversed order
                indices.Add(cur);
            }

            // ------------------------------------
            // SIDE WALLS (extrusion)
            // Connect front[i] → front[next]
            //          to back[i] → back[next]
            // ------------------------------------
            for (uint i = 0; i < 10; i++)
            {
                uint next = (i + 1) % 10;

                uint f0 = i;
                uint f1 = next;

                uint b0 = 11 + i;
                uint b1 = 11 + next;

                // First wall triangle
                indices.Add(f0);
                indices.Add(f1);
                indices.Add(b0);

                // Second wall triangle
                indices.Add(f1);
                indices.Add(b1);
                indices.Add(b0);
            }

            // Total indices stored for glDrawElements
            _indexCount = indices.Count;

            // ------------------------------------
            // Upload geometry to GPU buffers
            // ------------------------------------
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            // Upload vertex attributes
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Count * sizeof(float),
                vertices.ToArray(),
                BufferUsageHint.StaticDraw
            );

            // Upload triangle indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                indices.Count * sizeof(uint),
                indices.ToArray(),
                BufferUsageHint.StaticDraw
            );

            // Position attribute: 3 floats
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute: 3 floats
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Compile shaders
            _shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);

            // Basic camera setup
            _view = Matrix4.LookAt(new Vector3(2, 2, 3), Vector3.Zero, Vector3.UnitY);

            // Set projection
            Resize(width, height);
        }

        public void Update(float dt)
        {
            // Store elapsed time (used for animation)
            _time += dt;
        }

        public void Resize(int width, int height)
        {
            if (height == 0) height = 1;

            _aspectRatio = width / (float)height;

            // Update projection matrix to match window
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60),
                _aspectRatio,
                0.1f,
                100f
            );
        }

        public void Render()
        {
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            // Time-based rotation
            float t = (float)GLFW.GetTime();

            Matrix4 model =
                Matrix4.CreateRotationY(t * 0.8f) *
                Matrix4.CreateRotationX(t * 0.5f);

            // Upload matrices to the shader
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uModel"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uView"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "uProjection"), false, ref _projection);

            // Draw the entire star
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Unload()
        {
            // Cleanup GPU resources
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteProgram(_shaderProgram);
        }

        // ----------------- SHADERS -----------------

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

        // Compiles shaders and links them into a program
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

            // Delete shader objects after linking
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }
    }
}
