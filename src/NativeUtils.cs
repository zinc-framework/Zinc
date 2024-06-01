using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Zinc.Internal.Sokol;

namespace Zinc.NativeInterop;

internal static class Utils
{
    public class NativeArray<T> where T : unmanaged
    {
        public unsafe T* Ptr {get; protected set;}
        int len;
        int width; //this is used if you want to use 2D indexing into this array
        nuint elementSize;
        nuint size;
        public NativeArray(int size, bool defaultInit = true, int width = 1)
        {
            len = size;
            this.width = width; 
            if(len < 0)
            {
                throw new ArgumentOutOfRangeException("size", "Size must be a positive number");
            }
            unsafe
            {
                this.elementSize = (nuint)sizeof(T);
                this.size = (nuint)size;
                Ptr = (T*)NativeMemory.Alloc(this.size,this.elementSize);
                if (defaultInit)
                {
                    for (int i = 0; i < size; i++)
                    {
                        Ptr[i] = default(T);
                    }
                }
            }

        }

        public void Free()
        {
            unsafe
            {
                if(Ptr != null)
                {
                    NativeMemory.Free(Ptr);
                    Ptr = null;
                }
            }
        }

        public ref T this[int index]
        {
            get
            {
                if(index >= len || index < 0)
                {
                    throw new IndexOutOfRangeException("Array Index out of range");
                }
                unsafe
                {
                    // return ref Unsafe.AsRef<T>(Ptr + index);
                    return ref Ptr[index];
                }
            }
        }
        
        public ref T this[int x,int y] //indicies go from bottom left to top right
        {
            get
            {
                var index = (width * y) + x; 
                if(index >= len || index < 0)
                {
                    throw new IndexOutOfRangeException("Array Index out of range");
                }
                unsafe
                {
                    // return ref Unsafe.AsRef<T>(Ptr + index);
                    return ref Ptr[index];
                }
            }
        }

        public unsafe sg_range AsSgRange()
        {
            return new sg_range()
            {
                ptr = Ptr,
                size = elementSize * size
            };
        }

        ~NativeArray()
        {
            Free();
        }
    }
}