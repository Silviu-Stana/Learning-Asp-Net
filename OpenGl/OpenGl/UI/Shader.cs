using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

public class Shader
{
    public int Handle { get; private set; }

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexSource = File.ReadAllText(vertexPath);
        string fragmentSource = File.ReadAllText(fragmentPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
        GL.CompileShader(vertexShader);
        CheckCompileErrors(vertexShader, "VERTEX");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);
        GL.CompileShader(fragmentShader);
        CheckCompileErrors(fragmentShader, "FRAGMENT");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        CheckCompileErrors(Handle, "PROGRAM");

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void SetMatrix4(string name, Matrix4 mat)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, false, ref mat);
    }

    public void SetVector4(string name, Vector4 vec)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform4(location, vec);
    }

    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            Console.WriteLine($"Warning: uniform '{name}' not found.");
        GL.Uniform1(location, value);
    }

    private void CheckCompileErrors(int shader, string type)
    {
        if (type != "PROGRAM")
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                throw new Exception($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{info}");
            }
        }
        else
        {
            GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(shader);
                throw new Exception($"ERROR::PROGRAM_LINKING_ERROR of type: {type}\n{info}");
            }
        }
    }
}