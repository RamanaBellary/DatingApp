import { Photo } from "./photo"

export interface Member {
    Id: number
    UserName: string
    Age: number
    PhotoUrl: string
    KnownAs: string
    Created: Date
    LastActive: Date
    Gender: string
    Introduction: string
    Interests: any
    LookingFor: string
    City: string
    Country: string
    Photos: Photo[]
  }
  
  