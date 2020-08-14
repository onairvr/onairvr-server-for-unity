#import <UIKit/UIKit.h>
#import <OnCloudStreamKit/OCSUnityPlugin.h>
#import <UnityAppController.h>
#import "IUnityInterface.h"
#import "IUnityGraphics.h"

static OCSUnityPlugin *s_Plugin;

static void UNITY_INTERFACE_API PrepareRender(int eventID)       {}
static void UNITY_INTERFACE_API PreRenderVideoFrame(int eventID) {}
static void UNITY_INTERFACE_API EndOfRenderFrame(int eventID)    {}

static void UNITY_INTERFACE_API RenderVideoFrame(int eventID) {
    [s_Plugin renderWithEventID:eventID];
}

static void UNITY_INTERFACE_API SetRenderAspect(int eventID) {
    float aspect = eventID / 1000000.0f;
    [s_Plugin setRenderAspect:aspect];
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_Init(const char* licenseFilePath, int audioOutputSampleRate, bool hasInput) {
    return 1;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_Cleanup() {}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetProfile(const char* profile) {
    s_Plugin.profile = [NSString stringWithUTF8String:profile];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestConnect(const char* address, int port) {
    [s_Plugin connectTo:[NSString stringWithUTF8String:address] port:port];
}
    
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestDisconnect() {
    [s_Plugin disconnect];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestPlay() {
    [s_Plugin play];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestStop() {
    [s_Plugin stop];
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_IsConnected() {
    return [s_Plugin connected] == YES;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_IsPlaying() {
    return [s_Plugin playing] == YES;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetCameraOrientation(float x, float y, float z, float w, int* viewNumber) {}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetCameraProjection(float left, float top, float right, float bottom) {
    [s_Plugin setCameraProjectionWithLeft:left top:top right:right bottom:bottom];
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetAudioData(void* buffer, int length, int channels) {
    return false;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_CheckMessageQueue(void** source, const void** data, int* length) {
    struct OCSUnityPluginMessage msg;
    
    if ([s_Plugin peekMessage:&msg]) {
        *source = msg.source;
        *data = msg.data;
        *length = msg.length;
        return true;
    }
    return false;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RemoveFirstMessageFromQueue() {
    [s_Plugin popMessage];
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_DeviceRefreshRate() {
    return (int)UIScreen.mainScreen.maximumFramesPerSecond;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_HEVCDecodeSupported() {
    return [OCSUnityPlugin HEVCDecodeSupported];
}

unsigned char UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RegisterInputSender(const char* name) {
    return [s_Plugin registerInputSender:[NSString stringWithUTF8String:name]];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_UnregisterInputSender(unsigned char senderID) {
    [s_Plugin unregisterInputSender:senderID];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_BeginPendInput(long long* timestamp) {
    [s_Plugin beginPendInput:timestamp];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SendPendingInputs(long long timestamp) {
    [s_Plugin sendPendingInputs:timestamp];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_ResetInput() {
    [s_Plugin resetInput];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputTransform(unsigned char deviceID, unsigned char controlID, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, unsigned char policy) {
    float transform[7] = { posX, posY, posZ, rotX, rotY, rotZ, rotW };
    [s_Plugin pendInput:deviceID control:controlID values:transform length:7 policy:policy];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputFloat3(unsigned char deviceID, unsigned char controlID, float x, float y, float z, unsigned char policy) {
    float values[3] = { x, y, z };
    [s_Plugin pendInput:deviceID control:controlID values:values length:3 policy:policy];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
    s_Plugin = [[OCSUnityPlugin alloc] initWithUnityInterfaces:unityInterfaces];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
    s_Plugin = nil;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetRenderAspect_RenderThread_Func() {
    return SetRenderAspect;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PrepareRender_RenderThread_Func() {
    return PrepareRender;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PreRenderVideoFrame_RenderThread_Func() {
    return PreRenderVideoFrame;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RenderVideoFrame_RenderThread_Func() {
    return RenderVideoFrame;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_EndOfRenderFrame_RenderThread_Func() {
    return EndOfRenderFrame;
}

@interface OCSUnityAppController : UnityAppController
@end

@implementation OCSUnityAppController
- (void)shouldAttachRenderDelegate {
    // unlike desktops where plugin dynamic library is automatically loaded and registered
    // we need to do that manually on iOS
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end
IMPL_APP_CONTROLLER_SUBCLASS(OCSUnityAppController);
