
declare global {
  interface Window {
    chrome: {
      webview: {
        postMessage: (message: any) => void;
        addEventListener: (event: string, callback: (e: any) => void) => void;
      };
    };
  }
}
export {};