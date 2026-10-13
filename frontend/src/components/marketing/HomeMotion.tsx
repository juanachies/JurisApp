import { useEffect, useRef, useState, type MouseEvent as ReactMouseEvent, type ReactNode } from 'react'
import { cn } from '@/utils/cn'
import heroImage from '@/assets/hero-element.png'

export function HomeCursor() {
  const dot = useRef<HTMLDivElement>(null)
  const ring = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const fine = window.matchMedia('(pointer: fine)').matches
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    if (!fine || reduce) return

    let x = window.innerWidth / 2
    let y = window.innerHeight / 2
    let ringX = x
    let ringY = y
    let frame = 0

    const onMove = (event: MouseEvent) => {
      x = event.clientX
      y = event.clientY
    }

    const onOver = (event: MouseEvent) => {
      const target = event.target
      const hot = target instanceof Element && Boolean(target.closest('a, button'))
      ring.current?.classList.toggle('is-hot', hot)
    }

    const loop = () => {
      ringX += (x - ringX) * 0.16
      ringY += (y - ringY) * 0.16
      if (dot.current) dot.current.style.transform = `translate3d(${x}px, ${y}px, 0)`
      if (ring.current) ring.current.style.transform = `translate3d(${ringX}px, ${ringY}px, 0)`
      frame = requestAnimationFrame(loop)
    }

    document.documentElement.classList.add('home-cursor')
    window.addEventListener('mousemove', onMove)
    window.addEventListener('mouseover', onOver)
    frame = requestAnimationFrame(loop)

    return () => {
      cancelAnimationFrame(frame)
      document.documentElement.classList.remove('home-cursor')
      window.removeEventListener('mousemove', onMove)
      window.removeEventListener('mouseover', onOver)
    }
  }, [])

  return (
    <>
      <div ref={dot} className="home-cursor-dot" aria-hidden />
      <div ref={ring} className="home-cursor-ring" aria-hidden />
    </>
  )
}

export function Reveal({
  children,
  className,
  delay = 0,
}: {
  children: ReactNode
  className?: string
  delay?: number
}) {
  const ref = useRef<HTMLDivElement>(null)
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const element = ref.current
    if (!element) return
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      setVisible(true)
      return
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (!entry?.isIntersecting) return
        setVisible(true)
        observer.disconnect()
      },
      { threshold: 0.18 },
    )
    observer.observe(element)
    return () => observer.disconnect()
  }, [])

  return (
    <div
      ref={ref}
      className={cn('home-reveal', visible && 'is-in', className)}
      style={{ transitionDelay: `${delay}ms` }}
    >
      {children}
    </div>
  )
}

export function HeroShot() {
  const frame = useRef<HTMLDivElement>(null)
  const shot = useRef<HTMLImageElement>(null)

  const onMove = (event: ReactMouseEvent<HTMLDivElement>) => {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const bounds = frame.current?.getBoundingClientRect()
    const image = shot.current
    if (!bounds || !image) return
    const px = (event.clientX - bounds.left) / bounds.width - 0.5
    const py = (event.clientY - bounds.top) / bounds.height - 0.5
    image.style.transform = `rotateX(${py * -8}deg) rotateY(${px * 12}deg) translate3d(${px * 18}px, ${py * 12}px, 0)`
  }

  const onLeave = () => {
    if (shot.current) shot.current.style.transform = 'rotateX(0deg) rotateY(0deg) translate3d(0, 0, 0)'
  }

  return (
    <div ref={frame} className="home-stage relative" onMouseMove={onMove} onMouseLeave={onLeave}>
      <img
        ref={shot}
        src={heroImage}
        alt="Panel de JurisApp con casos, documentos y análisis"
        className="home-shot w-full"
      />
    </div>
  )
}
