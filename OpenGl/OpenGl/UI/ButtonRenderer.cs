using OpenGl.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ButtonRenderer
{
    private int _vao, _vbo;
    private Shader _shader;
    private Matrix4 _projection;
    public TextDrawer textDrawer;
    private int _framebufferWidth;
    private int _framebufferHeight;
    private string _text="";
    private float _x, _y;
    public ButtonRenderer(int windowWidth, int windowHeight)
    {
        _shader = new Shader("Assets/Shaders/button.vert", "Assets/Shaders/button.frag");
        textDrawer = new(windowWidth, windowHeight);

        // Simple VAO/VBO for dynamic rectangles
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

        GL.BindVertexArray(0);

        _projection = Matrix4.CreateOrthographicOffCenter(0, _framebufferWidth, 0, _framebufferHeight, -1, 1);

        Resize(windowWidth, windowHeight);
    }
    
    public void Resize(int framebufferWidth, int framebufferHeight)
    {
        _framebufferWidth = framebufferWidth;
        _framebufferHeight = framebufferHeight;

        GL.Viewport(0, 0, framebufferWidth, framebufferHeight);
        _projection = Matrix4.CreateOrthographicOffCenter(0, framebufferWidth, 0, framebufferHeight, -1, 1);

        textDrawer.Resize(framebufferWidth, framebufferHeight);
    }
    
    /// <summary>
    /// Draw a colored rectangle with optional rounded corners
    /// </summary>
    public void DrawButton(float x, float y, float width, float height, Vector4 color, float cornerRadius = 0, int cornerSegments = 8, string text="Triangle")
    {
        _x = x;
        _y = y;
        _text= text;

        var vertices = GenerateButtonVertices(x, y, width, height, cornerRadius, cornerSegments);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        _shader.Use();
        _shader.SetMatrix4("uProjection", _projection);
        _shader.SetVector4("uColor", color);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertices.Length / 2);
        GL.BindVertexArray(0);

        // Draw text on top
        textDrawer.DrawString(text, x + width / 2, y + height / 2);
    }

    /// <summary>
    /// Generates vertices for a rectangle with rounded corners as a triangle fan.
    /// </summary>
    private float[] GenerateButtonVertices(float x, float y, float w, float h, float r, int segments)
    {
        if (r <= 0)
        {
            // Simple rectangle: bottom-left, bottom-right, top-right, top-left
            return new float[]
            {
                x, y,
                x+w, y,
                x+w, y+h,
                x, y+h
            };
        }

        // Rounded rectangle: generate vertices around perimeter
        List<float> verts = new List<float>();
        float cx, cy, angleStep;

        // Bottom-left corner (180→270)
        cx = x + r; cy = y + r; angleStep = 90f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 180 + i * angleStep;
            float rad = MathF.PI / 180f * angle;
            verts.Add(cx + r * MathF.Cos(rad));
            verts.Add(cy + r * MathF.Sin(rad));
        }

        // Bottom-right (270→360)
        cx = x + w - r; cy = y + r;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 270 + i * angleStep;
            float rad = MathF.PI / 180f * angle;
            verts.Add(cx + r * MathF.Cos(rad));
            verts.Add(cy + r * MathF.Sin(rad));
        }

        // Top-right (0→90)
        cx = x + w - r; cy = y + h - r;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 0 + i * angleStep;
            float rad = MathF.PI / 180f * angle;
            verts.Add(cx + r * MathF.Cos(rad));
            verts.Add(cy + r * MathF.Sin(rad));
        }

        // Top-left (90→180)
        cx = x + r; cy = y + h - r;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 90 + i * angleStep;
            float rad = MathF.PI / 180f * angle;
            verts.Add(cx + r * MathF.Cos(rad));
            verts.Add(cy + r * MathF.Sin(rad));
        }

        return verts.ToArray();
    }
}
