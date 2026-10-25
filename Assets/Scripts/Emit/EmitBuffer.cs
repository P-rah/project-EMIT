using UnityEngine;
using System;

/*
    KeyFrame 저장은 FIFO, KeyFrame 꺼내기는 LIFO
    => Circular Queue로 구현해야 O(1)에 Insert, Pop 가능

*/
public class EmitBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _count;

    public int Count => _count;
    public int N => _buffer.Length;

    // 생성자
    public EmitBuffer(int N)
    {
        _buffer = new T[N];
    }

    public void Insert(T item)
    {
        _buffer[_head] = item;
        _head = (_head + 1) % N;

        if (_count < N)
        {
            _count++;
        }
    }

    public bool Pop(out T item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }

        _head = (_head - 1 + N) % N;
        item = _buffer[_head];
        _count--;

        return true;
    }

    public T getFrameAt(int index)
    {
        if (index < 0 || index >= _count)
        {
            return default;
        }

        int targetIndex = (((_head - 1 - index) % N) + N) % N;
        return _buffer[targetIndex];
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }
}
