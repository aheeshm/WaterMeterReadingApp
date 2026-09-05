import type { Page } from '@playwright/test';

type ImageFileOptions = {
    name: string;
    mimeType: 'image/png' | 'image/jpeg';
    sizeInBytes: number;
};

type UploadFile = {
    name: string;
    mimeType: string;
    buffer: Buffer;
};

const pngHeader = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
const pngFooter = Buffer.from('IEND');
const jpegHeader = Buffer.from([0xff, 0xd8, 0xff, 0xe0]);
const jpegFooter = Buffer.from([0xff, 0xd9]);

export function createImageFile(options: ImageFileOptions): UploadFile {
    const header = options.mimeType === 'image/png' ? pngHeader : jpegHeader;
    const footer = options.mimeType === 'image/png' ? pngFooter : jpegFooter;
    const padding = Math.max(options.sizeInBytes - header.length - footer.length, 0);

    return {
        name: options.name,
        mimeType: options.mimeType,
        buffer: Buffer.concat([header, Buffer.alloc(padding, 0x31), footer]),
    };
}

export async function fetchJson<T>(page: Page, path: string): Promise<T> {
    return page.evaluate(async (resourcePath) => {
        const response = await fetch(resourcePath);
        return response.json();
    }, path);
}
