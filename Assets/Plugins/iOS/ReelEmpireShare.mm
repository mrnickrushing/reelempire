// The share sheet: a picture (a PNG written by the game) and a line of text, shared however the player likes.
#import <UIKit/UIKit.h>

extern UIViewController *UnityGetGLViewController();

extern "C" {
void REShareImage(const char *path, const char *text) {
    NSString *p = path ? [NSString stringWithUTF8String:path] : nil;
    NSString *t = text ? [NSString stringWithUTF8String:text] : @"";
    NSMutableArray *items = [NSMutableArray array];
    if (t.length > 0) [items addObject:t];
    UIImage *image = p ? [UIImage imageWithContentsOfFile:p] : nil;
    if (image) [items addObject:image];
    if (items.count == 0) return;
    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *root = UnityGetGLViewController();
        UIActivityViewController *sheet = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil];
        // On an iPad the sheet is a popover and needs somewhere to point.
        sheet.popoverPresentationController.sourceView = root.view;
        sheet.popoverPresentationController.sourceRect = CGRectMake(CGRectGetMidX(root.view.bounds), CGRectGetMidY(root.view.bounds), 1, 1);
        sheet.popoverPresentationController.permittedArrowDirections = 0;
        [root presentViewController:sheet animated:YES completion:nil];
    });
}
}
