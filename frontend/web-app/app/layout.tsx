import './globals.css'
import type { Metadata } from 'next'
//import { Inter } from 'next/font/google'

// Components
import Navbar from './nav/Navbar'
import ToasterProvider from './providers/ToasterProvider'

//const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'Carsties',
  description: 'Car auctions',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  console.log("Server component");
  
  return (
    <html lang="en">
      <body>
        <ToasterProvider/>
        <Navbar/> 
        <main className='container mx-auto px-5 pt-5'>
          {children}
        </main>

        <footer className='min-h-32'>

        </footer>

        </body>
    </html>
  )
}
