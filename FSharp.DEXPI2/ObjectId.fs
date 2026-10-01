namespace FSharp.DEXPI2

type ObjectId = ObjectId of string
    with override this.ToString() = let (ObjectId s) = this in s
