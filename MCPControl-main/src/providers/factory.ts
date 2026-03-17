import { createRequire } from 'module';
import { AutomationProvider } from '../interfaces/provider.js';
import { AutoHotkeyProvider } from './autohotkey/index.js';
import { registry } from './registry.js';
import { AutomationConfig } from '../config.js';
import {
  KeyboardAutomation,
  MouseAutomation,
  ScreenAutomation,
  ClipboardAutomation,
} from '../interfaces/automation.js';

// Import individual providers
import { PowerShellClipboardProvider } from './clipboard/powershell/index.js';
import { ClipboardyProvider } from './clipboard/clipboardy/index.js';

// Cache to store provider instances
const providerCache: Record<string, AutomationProvider> = {};
const require = createRequire(import.meta.url);

function createDefaultProvider(): AutomationProvider {
  const providerType = (process.env.AUTOMATION_PROVIDER || 'keysender').toLowerCase();

  switch (providerType) {
    case 'autohotkey':
      return new AutoHotkeyProvider();
    case 'keysender': {
      // Try to load the keysender provider, but if the native dependency isn't
      // installed (common on non-Windows/dev machines) fall back to AutoHotkey
      // to allow the server to start.
      try {
        const { KeysenderProvider } = requireKeysenderProvider();
        return new KeysenderProvider();
      } catch (err) {
        // eslint-disable-next-line no-console
        console.warn(
          'Failed to load keysender provider, falling back to AutoHotkeyProvider:',
          err instanceof Error ? err.message : String(err),
        );
        return new AutoHotkeyProvider();
      }
    }
    default:
      throw new Error(`Unknown provider type: ${providerType}`);
  }
}

function requireKeysenderProvider(): { KeysenderProvider: new () => AutomationProvider } {
  try {
    return require('./keysender/index.js') as { KeysenderProvider: new () => AutomationProvider };
  } catch (err) {
    throw new Error(
      `Unable to require keysender provider. Make sure optional dependency 'keysender' is installed on Windows or configure a different provider. (${err instanceof Error ? err.message : String(err)})`,
    );
  }
}

/**
 * Initialize the provider registry with available providers
 */
export function initializeProviders(): void {
  // Register clipboard providers
  registry.registerClipboard('powershell', new PowerShellClipboardProvider());
  registry.registerClipboard('clipboardy', new ClipboardyProvider());

  // Register AutoHotkey providers
  const autohotkeyProvider = new AutoHotkeyProvider();
  registry.registerKeyboard('autohotkey', autohotkeyProvider.keyboard);
  registry.registerMouse('autohotkey', autohotkeyProvider.mouse);
  registry.registerScreen('autohotkey', autohotkeyProvider.screen);
  registry.registerClipboard('autohotkey', autohotkeyProvider.clipboard);

  // TODO: Register other providers as they are implemented
}

/**
 * Composite provider that allows mixing different component providers
 */
class CompositeProvider implements AutomationProvider {
  keyboard: KeyboardAutomation;
  mouse: MouseAutomation;
  screen: ScreenAutomation;
  clipboard: ClipboardAutomation;

  constructor(
    keyboard: KeyboardAutomation,
    mouse: MouseAutomation,
    screen: ScreenAutomation,
    clipboard: ClipboardAutomation,
  ) {
    this.keyboard = keyboard;
    this.mouse = mouse;
    this.screen = screen;
    this.clipboard = clipboard;
  }
}

/**
 * Create an automation provider instance based on configuration
 * Supports both legacy monolithic providers and new modular providers
 */
export function createAutomationProvider(config?: AutomationConfig): AutomationProvider {
  // Initialize providers if not already done
  if (registry.getAvailableProviders().clipboards.length === 0) {
    initializeProviders();
  }

  if (!config || !config.providers) {
    // Legacy behavior: use monolithic provider
    const type = config?.provider || 'keysender';
    const providerType = type.toLowerCase();

    // Return cached instance if available
    if (providerCache[providerType]) {
      return providerCache[providerType];
    }

    let provider: AutomationProvider;
    switch (providerType) {
      case 'autohotkey':
        provider = new AutoHotkeyProvider();
        break;
      case 'keysender': {
        try {
          const { KeysenderProvider } = requireKeysenderProvider();
          provider = new KeysenderProvider();
        } catch (err) {
          // Fallback to AutoHotkey if keysender cannot be loaded
          // eslint-disable-next-line no-console
          console.warn(
            'Failed to load keysender provider, falling back to AutoHotkeyProvider:',
            err instanceof Error ? err.message : String(err),
          );
          provider = new AutoHotkeyProvider();
        }
        break;
      }
      default:
        throw new Error(`Unknown provider type: ${providerType}`);
    }

    // Cache the instance
    providerCache[providerType] = provider;
    return provider;
  }

  // New modular approach
  const cacheKey = JSON.stringify(config.providers);
  if (providerCache[cacheKey]) {
    return providerCache[cacheKey];
  }

  // Get individual components from the registry
  const keyboardProvider = config.providers.keyboard
    ? registry.getKeyboard(config.providers.keyboard)
    : createDefaultProvider().keyboard;

  const mouseProvider = config.providers.mouse
    ? registry.getMouse(config.providers.mouse)
    : createDefaultProvider().mouse;

  const screenProvider = config.providers.screen
    ? registry.getScreen(config.providers.screen)
    : createDefaultProvider().screen;

  const clipboardProvider = config.providers.clipboard
    ? registry.getClipboard(config.providers.clipboard)
    : createDefaultProvider().clipboard;

  if (!keyboardProvider || !mouseProvider || !screenProvider || !clipboardProvider) {
    throw new Error('Failed to resolve all provider components');
  }

  const compositeProvider = new CompositeProvider(
    keyboardProvider,
    mouseProvider,
    screenProvider,
    clipboardProvider,
  );

  providerCache[cacheKey] = compositeProvider;
  return compositeProvider;
}
