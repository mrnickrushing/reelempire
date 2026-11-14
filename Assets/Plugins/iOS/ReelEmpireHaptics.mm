// Taptic feedback: a light tap for a button, a firmer one for an upgrade, the "success" pattern for a milestone.
#import <UIKit/UIKit.h>

extern "C" {
void REHapticImpact(int style) {
    UIImpactFeedbackGenerator *g = [[UIImpactFeedbackGenerator alloc] initWithStyle:(style <= 0 ? UIImpactFeedbackStyleLight : style == 1 ? UIImpactFeedbackStyleMedium : UIImpactFeedbackStyleHeavy)];
    [g prepare];
    [g impactOccurred];
}
void REHapticSuccess() {
    UINotificationFeedbackGenerator *g = [[UINotificationFeedbackGenerator alloc] init];
    [g prepare];
    [g notificationOccurred:UINotificationFeedbackTypeSuccess];
}
}
