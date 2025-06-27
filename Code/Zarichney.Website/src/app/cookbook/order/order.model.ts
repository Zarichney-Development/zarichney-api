export enum OrderStatus {
    Submitted = 0,
    InProgress = 1,
    Completed = 2,
    Paid = 3,
    Failed = 4,
    AwaitingPayment = 5
}

export enum PaymentStatus {
    None = 0,
    Pending = 1,
    Completed = 2,
    Failed = 3
}

export interface Order {
    orderId: string;
    recipeList: string[];
    synthesizedRecipes: SynthesizedRecipe[];
    status: OrderStatus;
    requiresPayment: boolean;
    customer: CustomerInfo;
    email: string;
    cookbookContent: CookbookContent;
    paymentStatus?: PaymentStatus;
    paymentSessionId?: string;
    price?: number;
}

export interface SynthesizedRecipe {
    title: string;
    description: string;
    servings: string;
    prepTime: string;
    cookTime: string;
    totalTime: string;
    ingredients: string[];
    directions: string[];
    notes: string;
    imageUrls: string[];
    qualityScore: number;
}

export interface CustomerInfo {
    email: string;
    availableRecipes: number;
    lifetimeRecipesUsed: number;
}

export interface CookbookContent {
    recipeSpecificationType: string;
    generalMealTypes: string[];
    expectedRecipeCount: number;
}
