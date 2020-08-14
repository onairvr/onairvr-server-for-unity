//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>

struct IUnityInterfaces;

typedef struct OCSUnityPluginMessage {
    void *source;
    const void *data;
    int length;
} OCSPluginMessage;

@interface OCSUnityPlugin : NSObject

@property (atomic) BOOL connected;
@property (atomic) BOOL playing;
@property (copy) NSString *profile;

- (instancetype)initWithUnityInterfaces:(struct IUnityInterfaces *)unityInterfaces;
- (void)connectTo:(NSString *)address port:(NSInteger)port;
- (void)disconnect;
- (void)play;
- (void)stop;
- (void)renderWithEventID:(int)eventID;
- (BOOL)peekMessage:(struct OCSUnityPluginMessage *)message;
- (void)popMessage;
- (uint8_t)registerInputSender:(NSString *)name;
- (void)unregisterInputSender:(uint8_t)senderID;
- (void)beginPendInput:(int64_t *)timestamp;
- (void)pendInput:(uint8_t)deviceID control:(uint8_t)controlID values:(const float *)values length:(int)length policy:(uint8_t)policy;
- (void)sendPendingInputs:(int64_t)timestamp;
- (void)resetInput;
- (void)setCameraProjectionWithLeft:(float)left top:(float)top right:(float)right bottom:(float)bottom;
- (void)setRenderAspect:(float)aspect;

+ (BOOL)HEVCDecodeSupported;

@end
