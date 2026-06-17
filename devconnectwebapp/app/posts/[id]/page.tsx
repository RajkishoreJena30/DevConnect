import PostDetailScreen from "@/app/posts/[id]/post-detail-screen";

export const dynamic = "force-dynamic";

export default async function PostDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const resolved = await params;
  return <PostDetailScreen postId={Number(resolved.id)} />;
}