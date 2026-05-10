#ifdef VERTEX
#version 450

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;

layout (set = 0, binding = 2) uniform Projection
{
    mat4 uProjection;
};

layout (location = 0) out vec2 vTexCoord;
layout (location = 1) out vec4 vColor;

void main()
{
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}
#endif

#ifdef FRAGMENT
#version 450

layout (set = 0, binding = 0) uniform texture2D uTexture;
layout (set = 0, binding = 1) uniform sampler uSampler;

layout (location = 0) in vec2 vTexCoord;
layout (location = 1) in vec4 vColor;

layout (location = 0) out vec4 outColor;

void main()
{
    vec4 color = texture(sampler2D(uTexture, uSampler), vTexCoord);
    outColor = vec4(color.rgb * vColor.rgb, color.a * vColor.a);
}
#endif