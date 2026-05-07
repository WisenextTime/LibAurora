#ifdef VERTEX
#version 450

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayerIndex;
layout (location = 3) in vec4 aColor;

layout (set = 0, binding = 2) uniform Projection
{
    mat4 uProjection;
};

layout (location = 0) out vec2 vTexCoord;
layout (location = 1) out flat float vLayerIndex;
layout (location = 2) out vec4 vColor;

void main()
{
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
    vLayerIndex = aLayerIndex;
    vColor = aColor;
}
#endif

#ifdef FRAGMENT
#version 450

layout (set = 0, binding = 0) uniform texture2DArray uFontAtlas;
layout (set = 0, binding = 1) uniform sampler uFontSampler;

layout (location = 0) in vec2 vTexCoord;
layout (location = 1) in flat float vLayerIndex;
layout (location = 2) in vec4 vColor;

layout (location = 0) out vec4 outColor;

void main()
{
    float alpha = texture(sampler2DArray(uFontAtlas, uFontSampler), vec3(vTexCoord, vLayerIndex)).r;
    outColor = vec4(vColor.rgb, vColor.a * alpha);
}
#endif