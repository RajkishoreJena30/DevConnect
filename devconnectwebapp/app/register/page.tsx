import AuthPage from "@/app/components/AuthPage";

export default async function RegisterPage({
  searchParams,
}: {
  searchParams: Promise<{ redirect?: string }>;
}) {
  const resolved = await searchParams;
  return <AuthPage mode="register" redirect={resolved.redirect || "/dashboard"} />;
}